﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace aspcore_async_deploy_smart_contract.Contract.Service
{
    public interface IBECInterface
    {
        Task<string> DeployContract(string accountAddress, string pw, string mastercontractaddr, string hash);
        Task<string> QuerryReceipt(string txId, int waitBeforeEachQuerry = 1000);
    }
}
