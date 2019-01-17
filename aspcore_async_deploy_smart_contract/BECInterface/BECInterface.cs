using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts.Managed;

using aspcore_async_deploy_smart_contract.Contract.Service;
using BECInterface.Contracts;
using aspcore_async_deploy_smart_contract.Contract.DTO;
using Microsoft.Extensions.Logging;
using aspcore_async_deploy_smart_contract;

namespace BECInterface
{
    public class BECInterface : IBECInterface
    {

        private readonly Web3 _web3;

        public IDictionary<string, ManagedAccount> ethereumAccounts;

        private readonly ILogger _logger;
        public BECInterface(ILoggerFactory loggerFactory)
        {
            //todo replace this with configurable value read from config file
            var account = new ManagedAccount(HardCodeData.accountAddr, HardCodeData.password);
            //set rpc client timeout to 1 000 000 ms
            ClientBase.ConnectionTimeout = new TimeSpan(0, 0, 0, 1_000_000);

            //WebSocketClient client = new WebSocketClient(hostAddress);
            RpcClient client = new RpcClient(new Uri(HardCodeData.hostAddress));
            _web3 = new Web3(client);

            _logger = loggerFactory.CreateLogger<IBECInterface>();
        }

        public async Task<TransactionResult> DeployContract(string accountAddress, string pw, string certId, Guid orgId, string hash)
        {
            /*
             * Clients retrieve the private key for an account (if stored on their keystore folder) using a password provided to decrypt the file. 
             * This is done when unlocking an account, or just at the time of sending a transaction if using personal_sendTransaction with a password.
             */
            bool isUnlocked = await _web3.Personal.UnlockAccount.SendRequestAsync(accountAddress, pw, 60);

            CertificationRegistryContract contract = new CertificationRegistryContract(_web3, accountAddress, HardCodeData.mastercontractaddr);

            var sha = new SHA512Managed();
            var tempBytes = Encoding.UTF8.GetBytes(hash);
            var hashByte = sha.ComputeHash(tempBytes);

            //todo ask henry chuong about orgId on blockchain
            var txId = await contract.SetIndividualCertificate(hashByte, certId, orgId.ToString("N"), 2_000_000_000);

            return new TransactionResult(certId, txId);
        }

        public async Task<ContractAddress> QueryReceipt(string certId, Guid orgId, string txId, int waitBeforeEachQuery = 1000)
        {
            TransactionReceipt receipt = default(TransactionReceipt);
            CertificationRegistryContract contract = new CertificationRegistryContract(_web3, HardCodeData.mastercontractaddr);

            while (true) {
                receipt = await _web3.Eth.Transactions.GetTransactionReceipt
                                    .SendRequestAsync(txId);

                var logs = receipt?.Logs;

                if (receipt != null) {
                    //todo ask henry chuong about orgId on blockchain
                    var certAddress = await contract.GetCertAddressByIdAsync(certId, orgId.ToString("N"));

                    return new ContractAddress(certId, certAddress);
                } else {
                    await Task.Delay(waitBeforeEachQuery);
                }
            }
        }
    }
}
