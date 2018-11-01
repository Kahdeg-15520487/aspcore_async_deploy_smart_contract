using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts.Managed;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BECInterface
{
    public class BECInterface
    {
        public readonly Web3 web3;
        public readonly SampleData sampleData;

        public BECInterface()
        {
            sampleData = new SampleData();

            var account = new ManagedAccount(sampleData.sender, sampleData.password);
            web3 = new Web3(account, sampleData.web3Host);
        }

        public async Task<string> DeployContract(string hash)
        {
            return await web3.Eth.DeployContract.SendRequestAsync(
                   sampleData.contractAbi,
                   sampleData.byteCode,
                   sampleData.sender,
                   sampleData.gasLimit,
                   null,
                   hash
               );
        }

        public async Task<(TransactionReceipt receipt, long runtime)> QuerryReceiptWithTxId(string txId, int waitBeforeEachQuerry = 1000)
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
                                        var receipt = t.Result;
                                        var runtime = timer.ElapsedMilliseconds;
                                        return (receipt, runtime);
                                    });

                if (result.receipt != null)
                {
                    timer.Stop();
                    return result;
                }
                else
                {
                    await Task.Delay(waitBeforeEachQuerry);
                }
            }
        }

        public async Task<(TransactionReceipt receipt, long runTime)[]> BulkDeployContract(IEnumerable<string> hashList)
        {
            List<(TransactionReceipt receipt, long runTime)> result = new List<(TransactionReceipt receipt, long runTime)>();
            IProgress<(TransactionReceipt receipt, long runtime)> progress =
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
                });

            return await BulkDeployContract(hashList, progress).ContinueWith(t =>
            {
                //todo: check for exception here
                //ex: t.Status == TaskStatus.Faulted ? -> handle t.Exception.InnerExceptions
                t.Wait();

                return result.ToArray();
            });
        }

        public async Task BulkDeployContract(IEnumerable<string> hashList, IProgress<(TransactionReceipt receipt, long runtime)> progress)
        {
            int waitBeforeEachQuerry = 1000;

            IProgress<string> txIdProgress =
                new Progress<string>(OnTransactionIdRequested(progress, waitBeforeEachQuerry));

            await BulkRequestTransactionId(hashList, txIdProgress);
        }

        private Action<string> OnTransactionIdRequested(IProgress<(TransactionReceipt receipt, long runtime)> progress, int waitBeforeEachQuerry)
        {
            return async (txId) =>
            {
                Console.WriteLine($"txId: {txId}");
                var value = await QuerryReceiptWithTxId(txId, waitBeforeEachQuerry);
                progress.Report(value);
            };
        }

        public async Task BulkRequestTransactionId(IEnumerable<string> hashList, IProgress<string> progress)
        {
            List<Task<string>> txIdTaskQuery = hashList.Select(hash =>
               web3.Eth.DeployContract.SendRequestAsync(
                   sampleData.contractAbi,
                   sampleData.byteCode,
                   sampleData.sender,
                   sampleData.gasLimit,
                   //todo set cancellation token?
                   null,
                   hash
               )).ToList();

            while (txIdTaskQuery.Count > 0)
            {
                //get the first completed task in the task list
                var completedTask = await Task.WhenAny(txIdTaskQuery);
                //remove it from the task list
                txIdTaskQuery.Remove(completedTask);

                //the querry result is here, maybe implement some kind of INotifier
                var txId = await completedTask;
                progress.Report(txId);
            }
        }
    }
}
