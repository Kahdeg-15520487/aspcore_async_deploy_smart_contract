using System;
using System.Collections.Generic;
using System.Text;

namespace aspcore_async_deploy_smart_contract.Contract.DTO
{
    public class TransactionResult
    {
        public TransactionResult(string certId, string txId)
        {
            CertificateId = certId;
            TxId = txId;
        }

        public string TxId { get; set; }
        public string CertificateId { get; set; }
        public override string ToString()
        {
            return string.Join(" : ", CertificateId, TxId);
        }
    }
}
