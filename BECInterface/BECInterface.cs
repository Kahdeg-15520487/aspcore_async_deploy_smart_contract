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
                   //todo remove cancellation token?
                   null,
                   hash
               );
        }

        public async Task BulkDeployContract(IEnumerable<string> hashList)
        {
            IProgress<(TransactionReceipt receipt, long runtime)> progress =
                new Progress<(TransactionReceipt receipt, long runtime)>(OnReceiptDone);

            await ShouldBulkDeployContractWithUsingIProgress(hashList, progress);
        }

        public void OnReceiptDone((TransactionReceipt receipt, long runtime) value)
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
            var reHashValue = hashFunc.CallAsync<string>().Result;

            //V invalid operation due to xunit claiming there is no test running
            Console.WriteLine($"hashValue:{reHashValue}, runTime:{runtime}");
        }

        public async Task ShouldBulkDeployContractWithUsingIProgress(IEnumerable<string> hashList, IProgress<(TransactionReceipt receipt, long runtime)> progress)
        {
            int waitBeforeEachQuerry = 1000;

            IProgress<string> txIdProgress =
                new Progress<string>(
                    async (txId) =>
                    {
                        Console.WriteLine($"txId: {txId}");
                        var value = await ShouldQuerryReceiptOnTxId(txId, waitBeforeEachQuerry);
                        progress.Report(value);
                    });

            await ShouldBulkRequestTransactionId(hashList, txIdProgress);
        }

        public async Task ShouldBulkRequestTransactionId(IEnumerable<string> hashList, IProgress<string> progress)
        {
            //var txId = await web3.Eth.DeployContract.SendRequestAsync(contractAbi, byteCode, sender, gasLimit, hashValue);
            List<Task<string>> txIdTaskQuery = hashList.Take(1).Select(hash =>
               web3.Eth.DeployContract.SendRequestAsync(
                   sampleData.contractAbi,
                   sampleData.byteCode,
                   sampleData.sender,
                   sampleData.gasLimit,
                   //todo remove cancellation token?
                   null,
                   hash
               )).ToList();

            while (txIdTaskQuery.Count > 0)
            {
                try
                {
                    //get the first completed task in the task list
                    var completedTask = await Task.WhenAny(txIdTaskQuery);
                    //remove it from the task list
                    txIdTaskQuery.Remove(completedTask);

                    //the quest result is here, maybe implement some kind of INotifier
                    var txId = await completedTask;
                    progress.Report(txId);
                }
                catch (Exception ex)
                {
                    //todo catch and log exception
                }
            }
        }

        public async Task<(TransactionReceipt receipt, long runtime)> ShouldQuerryReceiptOnTxId(string txId, int waitBeforeEachQuerry = 1000)
        {
            var timer = new Stopwatch();
            timer.Start();

            (TransactionReceipt receipt, long runtime) result = (null, 0);
            while (true)
            {
                result = await web3.Eth.Transactions.GetTransactionReceipt
                                    .SendRequestAsync(txId)
                                    .ContinueWith(async (t) =>
                                    {
                                        var receipt = await t;
                                        var runtime = timer.ElapsedMilliseconds;
                                        return (receipt, runtime);
                                    }).Result;

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
    }
}
