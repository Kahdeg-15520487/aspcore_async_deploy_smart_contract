using System;
using System.Collections.Generic;
using System.Text;

using aspcore_async_deploy_smart_contract.Contract.DTO;
using aspcore_async_deploy_smart_contract.Dal.Entities;

namespace aspcore_async_deploy_smart_contract.Contract.Service
{
    public interface IMapper
    {
        CertificateDTO Map(Certificate c);
    }
}
