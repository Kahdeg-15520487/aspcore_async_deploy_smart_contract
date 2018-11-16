using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using aspcore_async_deploy_smart_contract.Contract;
using aspcore_async_deploy_smart_contract.Contract.DTO;

using BECInterface;
using System.Linq;
using System.Collections.Generic;
using aspcore_async_deploy_smart_contract.Dal;
using aspcore_async_deploy_smart_contract.Dal.Entities;

namespace aspcore_async_deploy_smart_contract.AppService
{
    public class CertificateService : ICertificateService
    {
        private readonly BECInterface.BECInterface bec;
        private readonly IBackgroundTaskQueue<string> taskQueue;
        private readonly BECDbContext _context;

        public CertificateService(BECInterface.BECInterface bec, IBackgroundTaskQueue<string> taskQueue, BECDbContext context)
        {
            this.bec = bec;
            this.taskQueue = taskQueue;
            _context = context;
        }

        public async Task<IEnumerable<ReceiptQuerry>> BulkDeployContract(string[] hashList)
        {
            var result = await bec.BulkDeployContract(hashList);

            return result.Select(r =>
            {
                (Nethereum.RPC.Eth.DTOs.TransactionReceipt receipt, long runtime) = r;

                var contract = bec.web3.Eth.GetContract(bec.sampleData.contractAbi, receipt.ContractAddress);
                var hashFunc = contract.GetFunction("hashValue");
                var reHashValue = hashFunc.CallAsync<string>().Result;

                return new ReceiptQuerry()
                {
                    TransactionId = "",
                    Hash = reHashValue,
                    DeploymentTime = runtime
                };
            });
        }

        public void BulkDeployContractWithBackgroundTask(string[] hashs)
        {
            foreach (var hash in hashs)
            {
                taskQueue.QueueBackgroundWorkItem(async (ct) =>
                {
                    var task = bec.DeployContract(hash);
                    _context.Certificates.Add(new Certificate()
                    {
                        Id = Guid.NewGuid(),
                        TaskId = task.Id,
                        DeployStart = DateTime.Now,
                        DeployDone = default(DateTime),
                        Hash = hash,
                        Status = DeployStatus.Pending
                    });
                    return await task;
                });
            }
            return;
        }

        public async Task<string> DeployContract(string hash)
        {
            return await bec.DeployContract(hash);
        }

        public async Task<ReceiptQuerry> QuerryContractStatus(string txId)
        {
            return await bec.QuerryReceiptWithTxId(txId).ContinueWith(t =>
            {
                //check for exception
                if (t.Status == TaskStatus.Faulted)
                {
                    //todo handle exception
                    throw t.Exception;
                }

                (Nethereum.RPC.Eth.DTOs.TransactionReceipt receipt, long runtime) = t.Result;


                var contract = bec.web3.Eth.GetContract(bec.sampleData.contractAbi, receipt.ContractAddress);
                var hashFunc = contract.GetFunction("hashValue");
                var reHashValue = hashFunc.CallAsync<string>().Result;

                return new ReceiptQuerry()
                {
                    TransactionId = txId,
                    Hash = reHashValue,
                    DeploymentTime = runtime
                };
            });
        }
    }
}
