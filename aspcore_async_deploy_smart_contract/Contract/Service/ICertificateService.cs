using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using aspcore_async_deploy_smart_contract.Contract.DTO;

namespace aspcore_async_deploy_smart_contract.Contract.Service
{
    public interface ICertificateService
    {
        IEnumerable<CertificateDTO> GetCertificates();
        CertificateDTO GetCertificate(string txId);
        void BulkDeployContractWithBackgroundTask(string orgId,params string[] hashList);
        void DeleteAll();
    }
}
