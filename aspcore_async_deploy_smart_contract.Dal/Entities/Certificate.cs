using System;
using System.ComponentModel.DataAnnotations;

namespace aspcore_async_deploy_smart_contract.Dal.Entities
{
    public enum DeployStatus
    {
        Pending,
        ErrorInDeploy,
        DoneDeploying,
        DoneQuerrying
    }

    public class Certificate
    {
        [Key]
        public Guid Id { get; set; }

        public int TaskId { get; set; }

        public DateTime DeployStart { get; set; }
        public DateTime DeployDone { get; set; }

        public DeployStatus Status { get; set; }
        public string Messasge { get; set; }

        public string TransactionId { get; set; }
        public string Hash { get; set; }
    }
}
