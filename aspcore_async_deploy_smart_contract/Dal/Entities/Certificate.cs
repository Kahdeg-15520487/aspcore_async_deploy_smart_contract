using System;
using System.ComponentModel.DataAnnotations;

namespace aspcore_async_deploy_smart_contract.Dal.Entities
{
    public enum DeployStatus
    {
        Pending,
        ErrorInDeploy,
        Retrying,
        Querying,
        ErrorInQuerying,
        DoneQuerying
    }

    public class Certificate
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OrganizationId { get; set; }

        public string SmartContractAddress { get; set; }

        public DateTime DeployStart { get; set; }
        public DateTime DeployDone { get; set; }
        public DateTime QueryDone { get; set; }

        public DeployStatus SmartContractStatus { get; set; }
        public string Messasge { get; set; }

        public string TransactionId { get; set; }
        public string Hash { get; set; }
    }
}
