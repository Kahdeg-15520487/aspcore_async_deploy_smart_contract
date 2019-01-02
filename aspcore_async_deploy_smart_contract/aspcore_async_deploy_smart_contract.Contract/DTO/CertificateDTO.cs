using aspcore_async_deploy_smart_contract.Dal.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace aspcore_async_deploy_smart_contract.Contract.DTO
{
    public class CertificateDTO
    {
        public string TransactionId { get; set; }
        public DateTime DeployStart { get; set; }
        public DateTime DeployDone { get; set; }
        public long DeployTime { get; set; }
        public string DeployStatus { get; set; }
        public string Hash { get; set; }
        public string Message { get; set; }
    }
}
