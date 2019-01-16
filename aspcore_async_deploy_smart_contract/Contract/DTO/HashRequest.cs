using System;

namespace aspcore_async_deploy_smart_contract.Contract.DTO
{
    public class HashRequest
    {
        public Guid OrganizationId { get; set; }
        public string Hash { get; set; }
    }
}
