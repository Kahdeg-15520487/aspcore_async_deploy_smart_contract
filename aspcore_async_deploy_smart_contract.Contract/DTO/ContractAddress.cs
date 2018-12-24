using System;
using System.Collections.Generic;
using System.Text;

namespace aspcore_async_deploy_smart_contract.Contract.DTO
{
    public class ContractAddress
    {
        public ContractAddress(string certAddress)
        {
            ContractAddr = certAddress;
        }

        public string ContractAddr { get; set; }

        public override string ToString()
        {
            return ContractAddr;
        }
    }
}
