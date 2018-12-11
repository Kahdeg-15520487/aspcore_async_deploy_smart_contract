using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace aspcore_async_deploy_smart_contract.Contract.Service
{
    public interface IBECInterface<T>
    {
        Task<string> DeployContract(string hash);
        Task<T> QuerryReceipt(string txId, int waitBeforeEachQuerry = 1000);
    }
}
