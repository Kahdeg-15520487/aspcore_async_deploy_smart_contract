using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

using Nethereum.JsonRpc.Client;
//using Nethereum.JsonRpc.WebSocketClient;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts.Managed;
using Nethereum.Hex.HexTypes;

using aspcore_async_deploy_smart_contract.Contract.Service;
using BECInterface.Contracts;
using aspcore_async_deploy_smart_contract.Contract.DTO;
using Microsoft.Extensions.Logging;
using aspcore_async_deploy_smart_contract;

namespace BECInterface
{
    public class BECInterface : IBECInterface
    {
        public const string hostAddress = "http://10.8.0.1:8545/";
        //const string hostAddress = "ws://10.8.0.1:8546/";

        private Web3 _web3;

        private Web3 web3 {
            get {
                return _web3;
                //todo below is an attempt to check for web3 client liveness
                if (_web3 == null) {
                    RpcClient client = new RpcClient(new Uri(hostAddress));
                    _web3 = new Web3(client);
                }

                try {
                    _web3.Eth.Blocks.GetBlockTransactionCountByNumber.SendRequestAsync(BlockParameter.CreateLatest());
                } catch (RpcClientTimeoutException ex) {
                    RpcClient client = new RpcClient(new Uri(hostAddress));
                    _web3 = new Web3(client);
                    _logger.LogDebug("rpc client created");
                } catch (Exception ex) {
                    throw new Exception("", ex);
                }
                return _web3;
            }
        }

        public readonly SampleData sampleData;
        public IDictionary<string, ManagedAccount> ethereumAccounts;

        private readonly ILogger _logger;
        public BECInterface(ILoggerFactory loggerFactory)
        {
            sampleData = new SampleData();

            //todo replace this with configurable value read from config file
            var account = new ManagedAccount(HardCodeData.accountAddr, HardCodeData.password);
            //set rpc client timeout to 1 000 000 ms
            ClientBase.ConnectionTimeout = new TimeSpan(0, 0, 0, 1_000_000);

            //WebSocketClient client = new WebSocketClient(hostAddress);
            RpcClient client = new RpcClient(new Uri(hostAddress));
            _web3 = new Web3(client);

            _logger = loggerFactory.CreateLogger<IBECInterface>();
        }

        public async Task<TransactionResult> DeployContract(string accountAddress, string pw, string certId, Guid orgId, string hash)
        {
            /*
             * Clients retrieve the private key for an account (if stored on their keystore folder) using a password provided to decrypt the file. 
             * This is done when unlocking an account, or just at the time of sending a transaction if using personal_sendTransaction with a password.
             */
            bool isUnlocked = await web3.Personal.UnlockAccount.SendRequestAsync(accountAddress, pw, 60);

            CertificationRegistryContract contract = new CertificationRegistryContract(web3, accountAddress, HardCodeData.mastercontractaddr);

            var sha = new SHA512Managed();
            var tempBytes = Encoding.UTF8.GetBytes(hash);
            var hashByte = sha.ComputeHash(tempBytes);

            //todo ask henry chuong about orgId on blockchain
            var txId = await contract.SetIndividualCertificate(hashByte, certId, orgId.ToString("N"), 2_000_000_000);

            return new TransactionResult(certId, txId);
        }

        public async Task<ContractAddress> QueryReceipt(string certId, Guid orgId, string txId, int waitBeforeEachQuery = 1000)
        {
            TransactionReceipt receipt = default(TransactionReceipt);
            CertificationRegistryContract contract = new CertificationRegistryContract(web3, HardCodeData.mastercontractaddr);

            while (true) {
                receipt = await web3.Eth.Transactions.GetTransactionReceipt
                                    .SendRequestAsync(txId);

                var logs = receipt?.Logs;

                if (receipt != null) {
                    //todo ask henry chuong about orgId on blockchain
                    var certAddress = await contract.GetCertAddressByIdAsync(certId, orgId.ToString("N"));

                    return new ContractAddress(certId, certAddress);
                } else {
                    await Task.Delay(waitBeforeEachQuery);
                }
            }
        }

        [Obsolete]
        public async Task<(TransactionReceipt receipt, long runtime)> QueryReceiptWithTxId(string txId, int waitBeforeEachQuery = 1000, IProgress<(TransactionReceipt receipt, long runtime)> progress = null)
        {
            var timer = new Stopwatch();
            timer.Start();

            (TransactionReceipt receipt, long runtime) result = (null, 0);
            while (true) {
                result = await web3.Eth.Transactions.GetTransactionReceipt
                                    .SendRequestAsync(txId)
                                    .ContinueWith(t => {
                                        //todo handle exception here
                                        //there can be rpc connection error when 
                                        //somehow the geth instance is inaccessible
                                        try {
                                            var receipt = t.Result;
                                            var runtime = timer.ElapsedMilliseconds;
                                            return (receipt, runtime);
                                        } catch (Exception) {
                                            throw;
                                        }
                                    });

                if (result.receipt != null) {
                    timer.Stop();
                    progress?.Report(result);
                    return result;
                } else {
                    await Task.Delay(waitBeforeEachQuery);
                }
            }
        }

        [Obsolete]
        public async Task<(TransactionReceipt receipt, long runTime)[]> BulkDeployContract(IEnumerable<string> hashList, int waitBeforeEachQuery = 1000)
        {
            List<(TransactionReceipt receipt, long runTime)> result = new List<(TransactionReceipt receipt, long runTime)>();
            IProgress<(TransactionReceipt receipt, long runtime)> receiptProgress =
                new Progress<(TransactionReceipt receipt, long runtime)>(async (value) => {
                    var (receipt, runtime) = value;

                    //check null receipt
                    if (receipt is null) {
                        Console.WriteLine("null");
                        return;
                    }

                    var contract = web3.Eth.GetContract(sampleData.contractAbi, receipt.ContractAddress);
                    var hashFunc = contract.GetFunction("hashValue");
                    var reHashValue = await hashFunc.CallAsync<string>();

                    Console.WriteLine($"hashValue:{reHashValue}, runTime:{runtime}");

                    result.Add((receipt, runtime));
                });

            IProgress<string> txIdProgress =
                new Progress<string>(async (txId) => {
                    Console.WriteLine($"txId: {txId}");
                    //var value = await QueryReceiptWithTxId(txId, waitBeforeEachQuery);

                    //this IProgress approach:
                    // after each and every txid querried, start a receipt polling task
                    // immediately for that txid
                });

            var txIds = await BulkRequestTransactionId(hashList, txIdProgress);



            var reciptPollingTasks = txIds.Select(txId => {
                return QueryReceiptWithTxId(txId, waitBeforeEachQuery, progress: receiptProgress);
            }).ToArray();

            try {
                Task.WaitAll(reciptPollingTasks);
            } catch (AggregateException aex) {
                //todo handle exception in querry polling
            }

            //Console.WriteLine("finished task : {0}", reciptPollingTasks.Count(v => v.IsCompleted));

            //without v the result's count would not equal to the amount of finished task
            await Task.Delay(1000);

            return result.ToArray();
        }

        [Obsolete]
        public async Task<IEnumerable<string>> BulkRequestTransactionId(IEnumerable<string> hashList, IProgress<string> progress = null)
        {
            List<string> txIds = new List<string>();

            int CONCURRENCY_LEVEL = 5;
            int nextIndex = 0;
            List<Task<string>> txIdTasks = new List<Task<string>>();
            var hashs = hashList.ToList();

            Dictionary<int, string> taskHashlist = new Dictionary<int, string>();

            while (nextIndex < CONCURRENCY_LEVEL && nextIndex < hashs.Count()) {
                var t = web3.Eth.DeployContract.SendRequestAsync(
                      sampleData.contractAbi,
                      sampleData.byteCode,
                      sampleData.sender,
                      sampleData.gasLimit,
                      hashs[nextIndex]
                    );
                taskHashlist.Add(t.Id, hashs[nextIndex]);

                txIdTasks.Add(t);
                nextIndex++;
            }

            while (txIdTasks.Count > 0) {
                //get the first completed task in the task list
                var completedTask = await Task.WhenAny(txIdTasks);
                //remove it from the task list
                txIdTasks.Remove(completedTask);
                var hash = taskHashlist[completedTask.Id];
                taskHashlist.Remove(completedTask.Id);

                if (completedTask.IsFaulted) {
                    Console.WriteLine("faulted task's id: {0}", completedTask.Id);
                    Console.WriteLine("faulted task's hash: {0}", hash);
                    Console.WriteLine("Exception: {0}", string.Join(Environment.NewLine, completedTask.Exception.InnerExceptions.Select(ex => $"{ex.GetType().Name}{ex.Message}")));
                    //todo report errored hash
                    //maybe try it again later?
                } else {
                    //the querry result is here, maybe implement some kind of INotifier
                    var txId = await completedTask;
                    progress?.Report(txId);
                    txIds.Add(txId);
                }

                //queue another task
                if (nextIndex < hashs.Count) {
                    var t = web3.Eth.DeployContract.SendRequestAsync(
                          sampleData.contractAbi,
                          sampleData.byteCode,
                          sampleData.sender,
                          sampleData.gasLimit,
                          hashs[nextIndex]
                        );
                    taskHashlist.Add(t.Id, hashs[nextIndex]);

                    txIdTasks.Add(t);
                    nextIndex++;
                }
            }

            return txIds;
        }
    }
}
