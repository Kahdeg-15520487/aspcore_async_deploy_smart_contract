using System;
using System.Collections.Generic;
using System.Text;

namespace aspcore_async_deploy_smart_contract.Contract.DTO
{
    public class TransactionId
    {
        public TransactionId(string txId)
        {
            TxId = txId;
        }

        public string TxId { get; set; }

        public override string ToString()
        {
            return TxId;
        }
    }
}
