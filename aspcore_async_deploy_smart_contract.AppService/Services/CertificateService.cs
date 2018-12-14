using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Nethereum.RPC.Eth.DTOs;
using aspcore_async_deploy_smart_contract.Contract.DTO;
using aspcore_async_deploy_smart_contract.Dal;
using aspcore_async_deploy_smart_contract.Dal.Entities;
using aspcore_async_deploy_smart_contract.Contract.Service;

namespace aspcore_async_deploy_smart_contract.AppService
{
    public class CertificateService : ICertificateService
    {
        #region hardcode data
        const string accountAddr = "0x3382EfBCFA02461560cABD69530a6172255e8A67";
        const string password = "rosen";
        const string contractAddr = "0x1A7048CCA5224b5fe68036a8aE5189BcF76f7C09";
        #endregion


        private readonly IBECInterface bec;

        private readonly IBackgroundTaskQueue<(Guid id, Task<string> task)> taskQueue;

        private readonly BECDbContext _context;
        private readonly ILogger _logger;
        private readonly IMapper mapper;

        public CertificateService(IBECInterface bec, IBackgroundTaskQueue<(Guid id, Task<string> task)> taskQueue, BECDbContext context, ILoggerFactory loggerFactory, IMapper mapper)
        {
            this.bec = bec;
            this.taskQueue = taskQueue;
            _context = context;
            _logger = loggerFactory.CreateLogger<CertificateService>();
            this.mapper = mapper;
        }

        public IEnumerable<CertificateDTO> GetCertificates()
        {
            var tt = _context.Certificates.ToList();
            return tt.Select(c => mapper.Map(c));
        }

        public CertificateDTO GetCertificate(string txId)
        {
            var certificate = _context.Certificates.FirstOrDefault(cert => cert.Id.Equals(txId));

            if (certificate == null)
            {
                throw new KeyNotFoundException(txId);
            }

            return mapper.Map(certificate);
        }

        //public async Task<IEnumerable<ReceiptQuerry>> BulkDeployContract(string[] hashList)
        //{
        //    var result = await bec.BulkDeployContract(hashList);

        //    return result.Select(r =>
        //    {
        //        (Nethereum.RPC.Eth.DTOs.string receipt, long runtime) = r;

        //        var contract = bec.web3.Eth.GetContract(bec.sampleData.contractAbi, receipt.ContractAddress);
        //        var hashFunc = contract.GetFunction("hashValue");
        //        var reHashValue = hashFunc.CallAsync<string>().Result;

        //        return new ReceiptQuerry()
        //        {
        //            TransactionId = "",
        //            Hash = reHashValue,
        //            DeploymentTime = runtime
        //        };
        //    });
        //}

        public void BulkDeployContractWithBackgroundTask(string[] hashs)
        {
            foreach (var hash in hashs)
            {
                var certEntity = new Certificate()
                {
                    Id = Guid.NewGuid(),
                    DeployStart = DateTime.UtcNow,
                    DeployDone = default(DateTime),
                    Hash = hash,
                    Status = DeployStatus.Pending
                };
                _logger.LogInformation("Id: {0}, TaskId: {1}, hash: {2}", certEntity.Id, certEntity.TaskId, certEntity.Hash);

                _context.Certificates.Add(certEntity);
                _context.SaveChanges();

                taskQueue.QueueBackgroundWorkItem((ct) =>
                {
                    return bec.DeployContract(accountAddr, password, contractAddr, hash).ContinueWith(txid => (certEntity.Id, txid));
                });
            }
            return;
        }

        //public async Task<string> DeployContract(string hash)
        //{
        //    return await bec.DeployContract(hash);
        //}

        //public async Task<ReceiptQuerry> QuerryContractStatus(string txId)
        //{
        //    return await bec.QuerryReceiptWithTxId(txId).ContinueWith(t =>
        //    {
        //        //check for exception
        //        if (t.Status == TaskStatus.Faulted)
        //        {
        //            //todo handle exception
        //            throw t.Exception;
        //        }

        //        (Nethereum.RPC.Eth.DTOs.string receipt, long runtime) = t.Result;


        //        var contract = bec.web3.Eth.GetContract(bec.sampleData.contractAbi, receipt.ContractAddress);
        //        var hashFunc = contract.GetFunction("hashValue");
        //        var reHashValue = hashFunc.CallAsync<string>().Result;

        //        return new ReceiptQuerry()
        //        {
        //            TransactionId = txId,
        //            Hash = reHashValue,
        //            DeploymentTime = runtime
        //        };
        //    });
        //}
    }
}
