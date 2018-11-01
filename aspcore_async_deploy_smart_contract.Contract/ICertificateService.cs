using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using aspcore_async_deploy_smart_contract.Contract.DTO;

namespace aspcore_async_deploy_smart_contract.Contract
{
    public interface ICertificateService
    {
        Task<string> DeployContract(string hash);
        Task<ReceiptQuerry> QuerryContractStatus(string txId);
        Task<IEnumerable<ReceiptQuerry>> BulkDeployContract(string[] hashList);
    }
}
