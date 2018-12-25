using System;
using System.Collections.Generic;
using System.Text;

namespace aspcore_async_deploy_smart_contract.Contract.DTO
{
    public class ContractAddress
    {
        public ContractAddress(string certId, string certAddress)
        {
            CertificateId = certId;
            ContractAddr = certAddress;
        }

        public string ContractAddr { get; set; }
        public string CertificateId { get; set; }
        public override string ToString()
        {
            return string.Join(" : ",CertificateId, ContractAddr);
        }
    }
}
