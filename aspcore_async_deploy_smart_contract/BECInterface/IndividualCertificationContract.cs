using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Web3;
using Nethereum.Web3.Accounts.Managed;

namespace BECInterface.Contracts
{
    public class IndividualCertificationContract
    {
        public const string abi = "[{\"constant\":true,\"inputs\":[],\"name\":\"registryAddress\",\"outputs\":[{\"name\":\"\",\"type\":\"address\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"name\":\"_b0\",\"type\":\"bytes32\"},{\"name\":\"_b1\",\"type\":\"bytes32\"}],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"name\":\"newB0\",\"type\":\"bytes32\"},{\"indexed\":false,\"name\":\"newB1\",\"type\":\"bytes32\"}],\"name\":\"HashValueUpdated\",\"type\":\"event\"},{\"constant\":false,\"inputs\":[{\"name\":\"_b0\",\"type\":\"bytes32\"},{\"name\":\"_b1\",\"type\":\"bytes32\"}],\"name\":\"updateHashValue\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"hashValue\",\"outputs\":[{\"name\":\"\",\"type\":\"bytes32\"},{\"name\":\"\",\"type\":\"bytes32\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[],\"name\":\"deleteCertificate\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";

        public const string bytecode = "0x608060405234801561001057600080fd5b506040516040806103948339810180604052810190808051906020019092919080519060200190929190505050336000806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff1602179055508160018160001916905550806002816000191690555050506102f0806100a46000396000f300608060405260043610610062576000357c0100000000000000000000000000000000000000000000000000000000900463ffffffff1680632b92b8e5146100675780635f722859146100a9578063afa936b8146100e8578063ed9aab51146100ff575b600080fd5b34801561007357600080fd5b5061007c610156565b60405180836000191660001916815260200182600019166000191681526020019250505060405180910390f35b3480156100b557600080fd5b506100e660048036038101908080356000191690602001909291908035600019169060200190929190505050610167565b005b3480156100f457600080fd5b506100fd61022b565b005b34801561010b57600080fd5b5061011461029f565b604051808273ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200191505060405180910390f35b600080600154600254915091509091565b6000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff161415156101c257600080fd5b816001816000191690555080600281600019169055507f7a844e6aae238bcd98e7b13e2c45a101fcb726169853cc02211686360697f9e2828260405180836000191660001916815260200182600019166000191681526020019250505060405180910390a15050565b6000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff1614151561028657600080fd5b3273ffffffffffffffffffffffffffffffffffffffff16ff5b6000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff16815600a165627a7a723058205a45c17835ef786d41ec8598fe1967f69c76b7ffb889005c426f0433388335060029";

        private readonly string contractAddress;

        private readonly Web3 web3;

        private readonly ManagedAccount certAdminAccount;

        private readonly Contract contract;

        private const uint defaultGasLimit = 300000;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="_web3Conn">web3 connection</param>
        /// <param name="_certAdminAccount">account</param>
        /// <param name="_contractAddress">contract's address</param>
        public IndividualCertificationContract(
            Web3 _web3Conn,
            ManagedAccount _certAdminAccount,
            string _contractAddress)
        {
            web3 = _web3Conn;

            certAdminAccount = _certAdminAccount;

            contractAddress = _contractAddress;

            contract = web3.Eth.GetContract(abi, _contractAddress);
        }

        /// <summary>
        /// GetHashValue
        /// </summary>
        /// <returns></returns>
        public async Task<HashValueOutput> GetHashValue()
        {
            var thisFunction = contract.GetFunction("hashValue");
            return await thisFunction.CallAsync<HashValueOutput>();

        }

        [FunctionOutput]
        public class HashValueOutput
        {
            [Parameter("string", "b0", 1)]
            public string b0 { get; set; }

            [Parameter("string", "b1", 2)]
            public string b1 { get; set; }
        }
    }
}
