using System;
using System.Collections.Generic;
using System.Text;

using aspcore_async_deploy_smart_contract.Contract;
using aspcore_async_deploy_smart_contract.Contract.DTO;
using aspcore_async_deploy_smart_contract.Contract.Service;
using aspcore_async_deploy_smart_contract.Dal.Entities;

namespace aspcore_async_deploy_smart_contract.AppService
{
    class Mapper : IMapper
    {
        public CertificateDTO Map(Certificate c)
        {
            return new CertificateDTO()
            {
                TransactionId = c.TransactionId,
                DeployStart = c.DeployStart,
                DeployDone = c.DeployDone,
                DeployStatus = c.SmartContractStatus.ToString(),
                DeployTime = c.SmartContractStatus == DeployStatus.DoneQuerying ? (long)(c.DeployDone - c.DeployStart).TotalMilliseconds : 0,
                Hash = c.Hash,
                Message = c.Messasge
            };
        }
    }
}
