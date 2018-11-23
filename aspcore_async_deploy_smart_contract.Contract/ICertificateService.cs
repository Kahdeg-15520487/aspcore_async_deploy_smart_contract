using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using aspcore_async_deploy_smart_contract.Contract.DTO;

namespace aspcore_async_deploy_smart_contract.Contract
{
    public interface ICertificateService
    {
        IEnumerable<CertificateDTO> GetCertificates();
        CertificateDTO GetCertificate(string txId);
        Task<string> DeployContract(string hash);
        Task<ReceiptQuerry> QuerryContractStatus(string txId);
        Task<IEnumerable<ReceiptQuerry>> BulkDeployContract(string[] hashList);
        void BulkDeployContractWithBackgroundTask(string[] hashList);
    }
}
