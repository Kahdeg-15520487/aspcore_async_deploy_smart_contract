using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using aspcore_async_deploy_smart_contract.Contract;
using aspcore_async_deploy_smart_contract.Contract.DTO;

using BECInterface;

namespace aspcore_async_deploy_smart_contract.AppService
{
    public class CertificateService : ICertificateService
    {
        private readonly BECInterface.BECInterface bec;

        public CertificateService(BECInterface.BECInterface bec)
        {
            this.bec = bec;
        }

        public async Task<string> DeployContract(string hash)
        {
            return await bec.DeployContract(hash);
        }

        public async Task<ReceiptQuerry> QuerryContractStatus(string txId)
        {
            return await bec.ShouldQuerryReceiptOnTxId(txId).ContinueWith((value) =>
            {
                (Nethereum.RPC.Eth.DTOs.TransactionReceipt receipt, long runtime) = value.Result;


                var contract = bec.web3.Eth.GetContract(bec.sampleData.contractAbi, receipt.ContractAddress);
                var hashFunc = contract.GetFunction("hashValue");
                var reHashValue = hashFunc.CallAsync<string>().Result;

                return new ReceiptQuerry()
                {
                    TransactionId = txId,
                    Hash = reHashValue,
                    DeploymentTime = runtime
                };
            });
        }
    }
}
