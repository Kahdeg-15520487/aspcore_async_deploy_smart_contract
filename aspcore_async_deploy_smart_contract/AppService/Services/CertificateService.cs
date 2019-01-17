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
using Nethereum.Web3;
using Nethereum.Web3.Accounts.Managed;

namespace aspcore_async_deploy_smart_contract.AppService
{
    public class CertificateService : ICertificateService
    {
        private readonly IBECInterface bec;

        private readonly IBackgroundTaskQueue<(string id, Task<TransactionResult> task)> taskQueue;

        private readonly BECDbContext _context;
        private readonly ILogger _logger;
        private readonly IMapper mapper;

        public CertificateService(IBECInterface bec, IBackgroundTaskQueue<(string id, Task<TransactionResult> task)> taskQueue, BECDbContext context, ILoggerFactory loggerFactory, IMapper mapper)
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

            if (certificate == null) {
                throw new KeyNotFoundException(txId);
            }

            return mapper.Map(certificate);
        }

        //public async Task<IEnumerable<ReceiptQuery>> BulkDeployContract(string[] hashList)
        //{
        //    var result = await bec.BulkDeployContract(hashList);

        //    return result.Select(r =>
        //    {
        //        (Nethereum.RPC.Eth.DTOs.string receipt, long runtime) = r;

        //        var contract = bec.web3.Eth.GetContract(bec.sampleData.contractAbi, receipt.ContractAddress);
        //        var hashFunc = contract.GetFunction("hashValue");
        //        var reHashValue = hashFunc.CallAsync<string>().Result;

        //        return new ReceiptQuery()
        //        {
        //            TransactionResult = "",
        //            Hash = reHashValue,
        //            DeploymentTime = runtime
        //        };
        //    });
        //}

        public void BulkDeployContractWithBackgroundTask(Guid orgId,params string[] hashes)
        {
            foreach (var hash in hashes) {
                var certEntity = new Certificate() {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgId,
                    DeployStart = DateTime.UtcNow,
                    DeployDone = default(DateTime),
                    Hash = hash,
                    SmartContractStatus = DeployStatus.Pending
                };
                _logger.LogInformation("Id: {0}, hash: {1}", certEntity.Id, certEntity.Hash);

                _context.Certificates.Add(certEntity);
                _context.SaveChanges();
                var id = certEntity.Id;
                taskQueue.QueueBackgroundWorkItem((ct) => {
                    return bec.DeployContract(EthConnectionData.accountAddr, EthConnectionData.password, certEntity.Id.ToString(), orgId, hash).ContinueWith(txid => (id.ToString(), txid));
                });
            }
        }

        public void DeleteAll()
        {
            _context.Certificates.RemoveRange(_context.Certificates);
            _context.SaveChanges();
        }

        //public async Task<string> DeployContract(string hash)
        //{
        //    return await bec.DeployContract(hash);
        //}

        //public async Task<ReceiptQuery> QueryContractStatus(string txId)
        //{
        //    return await bec.QueryReceiptWithTxId(txId).ContinueWith(t =>
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

        //        return new ReceiptQuery()
        //        {
        //            TransactionResult = txId,
        //            Hash = reHashValue,
        //            DeploymentTime = runtime
        //        };
        //    });
        //}
    }
}
