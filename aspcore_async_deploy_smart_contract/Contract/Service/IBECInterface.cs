using aspcore_async_deploy_smart_contract.Contract.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Web3;

namespace aspcore_async_deploy_smart_contract.Contract.Service
{
    public interface IBECInterface
    {
        Task<TransactionResult> DeployContract(string accountAddress, string pw, string certId, Guid orgId, string hash);
        Task<ContractAddress> QueryReceipt(string certId, Guid orgId, string txId, int waitBeforeEachQuery = 1000);
    }
}
