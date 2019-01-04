namespace aspcore_async_deploy_smart_contract.Contract.DTO
{
    public class ReceiptQuerry
    {
        public string TransactionId { get; set; }
        public string Hash { get; set; }
        public long DeploymentTime { get; set; }
    }
}