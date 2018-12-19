using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts.Managed;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using aspcore_async_deploy_smart_contract.Contract.Service;
using BECInterface.Contracts;
using Nethereum.Hex.HexTypes;
using System.Security.Cryptography;
using System.Text;

namespace BECInterface
{
    public class BECInterface : IBECInterface
    {
        const string hostAddress = "http://10.8.0.1:8545/";
        const string mastercontractaddr = "0x1A7048CCA5224b5fe68036a8aE5189BcF76f7C09";

        public readonly Web3 web3;
        public readonly SampleData sampleData;
        public IDictionary<string, ManagedAccount> ethereumAccounts;
        public BECInterface()
        {
            sampleData = new SampleData();

            var account = new ManagedAccount(sampleData.sender, sampleData.password);
            //set rpc client timeout to 1 000 000 ms
            ClientBase.ConnectionTimeout = new TimeSpan(0, 0, 0, 1_000_000);
            web3 = new Web3(account, sampleData.web3Host);
        }

        public async Task<string> DeployContract(string accountAddress, string pw, string certId, string orgId, string hash)
        {
            ManagedAccount account = new ManagedAccount(accountAddress, pw);
            Web3 w3conn = new Web3(hostAddress);

            bool isUnlocked = await w3conn.Personal.UnlockAccount.SendRequestAsync(accountAddress, pw, 60);

            CertificationRegistryContract contract = new CertificationRegistryContract(w3conn, account, mastercontractaddr);

            var sha = new SHA512Managed();
            var tempBytes = Encoding.UTF8.GetBytes(hash);
            var hashByte = sha.ComputeHash(tempBytes);

            return await contract.SetIndividualCertificate(certId, hashByte, orgId, 2_000_000_000);
            //return await web3.Eth.DeployContract.SendRequestAsync(
            //       sampleData.contractAbi,
            //       sampleData.byteCode,
            //       sampleData.sender,
            //       sampleData.gasLimit,
            //       null,
            //       hash
            //   );
        }

        public async Task<string> QuerryReceipt(string certId, string orgId, string txId, int waitBeforeEachQuerry = 1000)
        {
            TransactionReceipt receipt = default(TransactionReceipt);
            Web3 w3conn = new Web3(hostAddress);

            CertificationRegistryContract contract = new CertificationRegistryContract(w3conn, mastercontractaddr);

            while (true)
            {
                receipt = await w3conn.Eth.Transactions.GetTransactionReceipt
                                    .SendRequestAsync(txId);

                var logs = receipt?.Logs;

                if (receipt != null)
                {
                    //var events = await contract.GetCertificationSetEvent(new BlockParameter(receipt.BlockNumber));
                    var certAddress = await contract.GetCertAddressByIdAsync(certId, orgId);

                    return certAddress;
                }
                else
                {
                    await Task.Delay(waitBeforeEachQuerry);
                }
            }
        }

        [Obsolete]
        public async Task<(TransactionReceipt receipt, long runtime)> QuerryReceiptWithTxId(string txId, int waitBeforeEachQuerry = 1000, IProgress<(TransactionReceipt receipt, long runtime)> progress = null)
        {
            var timer = new Stopwatch();
            timer.Start();

            (TransactionReceipt receipt, long runtime) result = (null, 0);
            while (true)
            {
                result = await web3.Eth.Transactions.GetTransactionReceipt
                                    .SendRequestAsync(txId)
                                    .ContinueWith(t =>
                                    {
                                        //todo handle exception here
                                        //there can be rpc connection error when 
                                        //somehow the geth instance is inaccessible
                                        try
                                        {
                                            var receipt = t.Result;
                                            var runtime = timer.ElapsedMilliseconds;
                                            return (receipt, runtime);
                                        }
                                        catch (Exception)
                                        {
                                            throw;
                                        }
                                    });

                if (result.receipt != null)
                {
                    timer.Stop();
                    progress?.Report(result);
                    return result;
                }
                else
                {
                    await Task.Delay(waitBeforeEachQuerry);
                }
            }
        }

        [Obsolete]
        public async Task<(TransactionReceipt receipt, long runTime)[]> BulkDeployContract(IEnumerable<string> hashList, int waitBeforeEachQuerry = 1000)
        {
            List<(TransactionReceipt receipt, long runTime)> result = new List<(TransactionReceipt receipt, long runTime)>();
            IProgress<(TransactionReceipt receipt, long runtime)> receiptProgress =
                new Progress<(TransactionReceipt receipt, long runtime)>(async (value) =>
                {
                    var (receipt, runtime) = value;

                    //check null receipt
                    if (receipt is null)
                    {
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
                new Progress<string>(async (txId) =>
                {
                    Console.WriteLine($"txId: {txId}");
                    //var value = await QuerryReceiptWithTxId(txId, waitBeforeEachQuerry);

                    //this IProgress approach:
                    // after each and every txid querried, start a receipt polling task
                    // immediately for that txid
                });

            var txIds = await BulkRequestTransactionId(hashList, txIdProgress);



            var reciptPollingTasks = txIds.Select(txId =>
            {
                return QuerryReceiptWithTxId(txId, waitBeforeEachQuerry, progress: receiptProgress);
            }).ToArray();

            try
            {
                Task.WaitAll(reciptPollingTasks);
            }
            catch (AggregateException aex)
            {
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

            while (nextIndex < CONCURRENCY_LEVEL && nextIndex < hashs.Count())
            {
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

            while (txIdTasks.Count > 0)
            {
                //get the first completed task in the task list
                var completedTask = await Task.WhenAny(txIdTasks);
                //remove it from the task list
                txIdTasks.Remove(completedTask);
                var hash = taskHashlist[completedTask.Id];
                taskHashlist.Remove(completedTask.Id);

                if (completedTask.IsFaulted)
                {
                    Console.WriteLine("faulted task's id: {0}", completedTask.Id);
                    Console.WriteLine("faulted task's hash: {0}", hash);
                    Console.WriteLine("Exception: {0}", string.Join(Environment.NewLine, completedTask.Exception.InnerExceptions.Select(ex => $"{ex.GetType().Name}{ex.Message}")));
                    //todo report errored hash
                    //maybe try it again later?
                }
                else
                {
                    //the querry result is here, maybe implement some kind of INotifier
                    var txId = await completedTask;
                    progress?.Report(txId);
                    txIds.Add(txId);
                }

                //queue another task
                if (nextIndex < hashs.Count)
                {
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
