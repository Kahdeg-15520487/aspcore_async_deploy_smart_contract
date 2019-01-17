using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts.Managed;

namespace BECInterface.Contracts
{

    public class CertificationRegistryContract
    {
        public const string abi =
            "[{\"constant\":false,\"inputs\":[{\"name\":\"_GlobalAdmin\",\"type\":\"address\"}],\"name\":\"changeGlobalAdmin\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"name\":\"_certAdmin\",\"type\":\"address\"},{\"name\":\"_organizationID\",\"type\":\"string\"}],\"name\":\"getCertAdminByOrganizationID\",\"outputs\":[{\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_CertID\",\"type\":\"string\"}],\"name\":\"delOrganizationCertificate\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"name\":\"_CertID\",\"type\":\"string\"},{\"name\":\"_organizationID\",\"type\":\"string\"}],\"name\":\"toCertificateKey\",\"outputs\":[{\"name\":\"\",\"type\":\"bytes32\"}],\"payable\":false,\"stateMutability\":\"pure\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"name\":\"\",\"type\":\"bytes32\"}],\"name\":\"CertAdmins\",\"outputs\":[{\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_CertAdmin\",\"type\":\"address\"}],\"name\":\"delRosenCertAdmin\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_CertAdmin\",\"type\":\"address\"}],\"name\":\"addRosenCertAdmin\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"name\":\"_organizationID\",\"type\":\"string\"},{\"name\":\"_CertID\",\"type\":\"string\"}],\"name\":\"getCertAddressByID\",\"outputs\":[{\"name\":\"\",\"type\":\"address\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_CertAdmin\",\"type\":\"address\"},{\"name\":\"_organizationID\",\"type\":\"string\"}],\"name\":\"addCertAdmin\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_CertID\",\"type\":\"string\"},{\"name\":\"_organizationID\",\"type\":\"string\"}],\"name\":\"delIndividualCertificate\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_CompanyName\",\"type\":\"string\"},{\"name\":\"_Norm\",\"type\":\"string\"},{\"name\":\"_CertID\",\"type\":\"string\"},{\"name\":\"_issued\",\"type\":\"uint256\"},{\"name\":\"_expires\",\"type\":\"uint256\"},{\"name\":\"_Scope\",\"type\":\"string\"},{\"name\":\"_issuingBody\",\"type\":\"string\"}],\"name\":\"setCertificate\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"GlobalAdmin\",\"outputs\":[{\"name\":\"\",\"type\":\"address\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"name\":\"_certAdmin\",\"type\":\"address\"},{\"name\":\"_organizationID\",\"type\":\"string\"}],\"name\":\"toCertAdminKey\",\"outputs\":[{\"name\":\"\",\"type\":\"bytes32\"}],\"payable\":false,\"stateMutability\":\"pure\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"name\":\"\",\"type\":\"bytes32\"}],\"name\":\"RosenCertificateAddresses\",\"outputs\":[{\"name\":\"\",\"type\":\"address\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"_CertAdmin\",\"type\":\"address\"},{\"name\":\"_organizationID\",\"type\":\"string\"}],\"name\":\"delCertAdmin\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"name\":\"_CertID\",\"type\":\"string\"}],\"name\":\"getOrganizationalCertAddressByID\",\"outputs\":[{\"name\":\"\",\"type\":\"address\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"name\":\"\",\"type\":\"address\"}],\"name\":\"RosenCertAdmins\",\"outputs\":[{\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"b0\",\"type\":\"bytes32\"},{\"name\":\"b1\",\"type\":\"bytes32\"},{\"name\":\"_CertID\",\"type\":\"string\"},{\"name\":\"_organizationID\",\"type\":\"string\"}],\"name\":\"setIndividualCertificate\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"name\":\"\",\"type\":\"bytes32\"}],\"name\":\"CertificateAddresses\",\"outputs\":[{\"name\":\"\",\"type\":\"address\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"b0\",\"type\":\"bytes32\"},{\"name\":\"b1\",\"type\":\"bytes32\"},{\"name\":\"_CertID\",\"type\":\"string\"},{\"name\":\"_organizationID\",\"type\":\"string\"}],\"name\":\"updateIndividualCertificate\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"name\":\"contractAddress\",\"type\":\"address\"}],\"name\":\"CertificationSet\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"name\":\"contractAddress\",\"type\":\"address\"}],\"name\":\"IndividualCertificationSet\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"name\":\"contractAddress\",\"type\":\"address\"}],\"name\":\"IndividualCertificationUpdated\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"name\":\"contractAddress\",\"type\":\"address\"}],\"name\":\"CertificationDeleted\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"name\":\"account\",\"type\":\"address\"}],\"name\":\"CertAdminAdded\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"name\":\"account\",\"type\":\"address\"}],\"name\":\"CertAdminDeleted\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"name\":\"account\",\"type\":\"address\"}],\"name\":\"GlobalAdminChanged\",\"type\":\"event\"}]";

        private readonly string contractAddress;

        private readonly Web3 web3;

        private readonly string certAdminAccountAddress;

        private readonly Contract contract;

        private const uint defaultGasLimit = 300000;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="_web3Conn">web3 connection</param>
        /// <param name="_certAdminAccount">account</param>
        /// <param name="_contractAddress">master contract address</param>
        public CertificationRegistryContract(
            Web3 _web3Conn,
            string _certAdminAccountAddress,
            string _contractAddress)
        {
            web3 = _web3Conn;

            certAdminAccountAddress = _certAdminAccountAddress;

            contractAddress = _contractAddress;

            contract = web3.Eth.GetContract(abi, _contractAddress);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_web3Connn"></param>
        /// <param name="_contractAddress"></param>
        public CertificationRegistryContract(
            Web3 _web3Connn,
            string _contractAddress)
        {
            web3 = _web3Connn;

            contract = web3.Eth.GetContract(abi, _contractAddress);
        }

        /// <summary>
        /// set a organizaional cert
        /// </summary>
        /// <param name="companyName"></param>
        /// <param name="norm"></param>
        /// <param name="certId"></param>
        /// <param name="issuedDate"></param>
        /// <param name="expireDate"></param>
        /// <param name="scope"></param>
        /// <param name="issuingBody"></param>
        /// <param name="gasPrice"></param>
        /// <returns></returns>
        public Task<string> SetCertificate(
            string companyName,
            string norm,
            string certId,
            uint issuedDate,
            uint expireDate,
            string scope,
            string issuingBody,
            uint gasPrice
        )
        {
            var thisFunction = contract.GetFunction("setCertificate");
            var deployTask = thisFunction.SendTransactionAsync(
                certAdminAccountAddress,
                new HexBigInteger(defaultGasLimit),
                new HexBigInteger(gasPrice),
                null,
                new object[] { companyName, norm, certId, issuedDate, expireDate, scope, issuingBody });
            return deployTask;
        }

        /// <summary>
        /// set an individual cert
        /// </summary>
        /// <param name="certId">cert's id</param>
        /// <param name="hashValue">64 bytes of the cert's hash</param>
        /// <param name="organizationId"></param>
        /// <param name="gasPrice"></param>
        /// <returns>Transaction Id</returns>
        public Task<string> SetIndividualCertificate(
            byte[] hashValue,
            string certId,
            string organizationId, // should contain maximum 32 characters
            uint gasPrice)
        {
            byte[] firstBytes32 = new byte[32];
            byte[] lastBytes32 = new byte[32];
            Array.Copy(hashValue, firstBytes32, 32);
            Array.Copy(hashValue, 32, lastBytes32, 0, 32);

            var thisFunction = contract.GetFunction("setIndividualCertificate");
            var deployTask = thisFunction.SendTransactionAsync(
                certAdminAccountAddress,
                new HexBigInteger(defaultGasLimit),
                new HexBigInteger(gasPrice),
                null,
                new object[] { firstBytes32, lastBytes32, certId, organizationId });
            return deployTask;
        }

        /// <summary>
        /// update an individual cert
        /// </summary>
        /// <param name="certId">cert's id</param>
        /// <param name="hashValue">64 bytes of the cert's hash</param>
        /// <param name="organizationId"></param>
        /// <param name="gasPrice"></param>
        /// <returns></returns>
        public Task<string> UpdateIndividualCertificate(
            byte[] hashValue,
            string certId,
            string organizationId, // should contain maximum 32 characters
            uint gasPrice)
        {
            
            byte[] firstBytes32 = new byte[32];
            byte[] lastBytes32 = new byte[32];
            Array.Copy(hashValue, firstBytes32, 32);
            Array.Copy(hashValue, 32, lastBytes32, 0, 32);

            var thisFunction = contract.GetFunction("updateIndividualCertificate");
            var deployTask = thisFunction.SendTransactionAsync(
                certAdminAccountAddress,
                new HexBigInteger(defaultGasLimit),
                new HexBigInteger(gasPrice),
                null,
                new object[] { firstBytes32, lastBytes32, certId, organizationId });
            return deployTask;
        }

        /// <summary>
        /// add a cert admin
        /// </summary>
        /// <param name="address">cert admin's address</param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public Task<string> AddCertAdmin(
            string address,
            string organizationId
        )
        { 
            var thisFunction = contract.GetFunction("addCertAdmin");
            var addCertAdminTask = thisFunction.SendTransactionAsync(
                certAdminAccountAddress,
                new HexBigInteger(defaultGasLimit),
                null,
                new object[] { address, organizationId });
            return addCertAdminTask;

        }
        /// <summary>
        /// add a rosen cert admin
        /// </summary>
        /// <param name="address"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public Task<string> AddRosenCertAdmin(
            string address
        )
        {
            var thisFunction = contract.GetFunction("addRosenCertAdmin");
            var addCertAdminTask = thisFunction.SendTransactionAsync(
                certAdminAccountAddress,
                new HexBigInteger(defaultGasLimit),
                null,
                new object[] { address });
            return addCertAdminTask;
        }
        /// <summary>
        /// delete a cert admin
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Task<string> DelCertAdmin(
            string address,
            string organizationId
        )
        {
            var thisFunction = contract.GetFunction("delCertAdmin");
            var delAdminTask = thisFunction.SendTransactionAsync(
                certAdminAccountAddress,
                new HexBigInteger(defaultGasLimit),
                null,
                new object[] { address, organizationId });
            return delAdminTask;

        }
        /// <summary>
        /// delete a rosen cert admin
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Task<string> DelRosenCertAdmin(
            string address
        )
        {
            var thisFunction = contract.GetFunction("delRosenCertAdmin");
            var delAdminTask = thisFunction.SendTransactionAsync(
                certAdminAccountAddress,
                new HexBigInteger(defaultGasLimit),
                null,
                new object[] { address });
            return delAdminTask;

        }

        /// <summary>
        /// get a cert address by its id
        /// </summary>
        /// <param name="_certID"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public Task<string> GetCertAddressById(
            string _certID,
            string organizationId
        )
        {
            var thisFunction = contract.GetFunction("getCertAddressByID");
            return thisFunction.CallAsync<string>(_certID, organizationId);
        }

        /// <summary>
        /// async get a cert address by its id
        /// </summary>
        /// <param name="certID"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public async Task<string> GetCertAddressByIdAsync(
            string certID,
            string organizationId
        )
        {
            var thisFunction = contract.GetFunction("getCertAddressByID");
            return await thisFunction.CallAsync<string>(certID, organizationId);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_certID"></param>
        /// <returns></returns>
        public Task<string> GetOrganizationalCertAddressByID(
            string _certID
        )
        {
            var thisFunction = contract.GetFunction("getOrganizationalCertAddressByID");
            return thisFunction.CallAsync<string>(_certID);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_certID"></param>
        /// <returns></returns>
        public async Task<string> GetOrganizationalCertAddressByIDAsync(
            string _certID
        )
        {
            var thisFunction = contract.GetFunction("getOrganizationalCertAddressByID");
            return await thisFunction.CallAsync<string>(_certID);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="_certID"></param>
        /// <returns></returns>
        public async Task<string> GetCertificateAddressByOrgIdAndCertIdAsync(
            string organizationId,
            string _certID
        )
        {
            var thisFunction = contract.GetFunction("CertificateAddresses");
            var getCertKeyFunc = contract.GetFunction("getCertKey");
            var shaCertKey = await getCertKeyFunc.CallAsync<string>(_certID);
            return await thisFunction.CallAsync<string>(organizationId, shaCertKey);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_certID"></param>
        /// <returns></returns>
        public async Task<string> GetRosenCertificateAddressByCertIdAsync(
            string _certID
        )
        {
            var thisFunction = contract.GetFunction("RosenCertificateAddresses");
            var getCertKeyFunc = contract.GetFunction("getCertKey");
            var shaCertKey = await getCertKeyFunc.CallAsync<string>(_certID);
            return await thisFunction.CallAsync<string>(shaCertKey);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task<bool> CheckCertAdminByOrgIdAndAddressAsync(
            string organizationId,
            string address
        )
        {
            if (!address.StartsWith("0x"))
            {
                throw new Exception("Invalid function input");
            }
            var thisFunction = contract.GetFunction("CertAdmins");
            return await thisFunction.CallAsync<bool>(organizationId, address);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_certID"></param>
        /// <returns></returns>
        public async Task<bool> CheckRosenCertAdminByAddressAsync(
            string _certID
        )
        {
            var thisFunction = contract.GetFunction("RosenCertAdmins");
            return await thisFunction.CallAsync<bool>(_certID);
        }

        /// <summary>
        /// GetCertificationSetEvent
        /// </summary>
        /// <returns>a list of event</returns>
        public async Task<IEnumerable<IndividualCertificationSet>> GetIndividualCertificationSetEvent(BlockParameter fromBlock = null)
        {
            var thisEvent = contract.GetEvent<IndividualCertificationSet>("IndividualCertificationSet");
            var filterBlockNumber = await thisEvent.CreateFilterAsync(fromBlock);
            var allEvents = await thisEvent.GetFilterChanges(filterBlockNumber);
            return allEvents.Select(el => el.Event);
        }
    }

    [Event("IndividualCertificationSet")]
    public class IndividualCertificationSet : IEventDTO
    { 
        [Parameter("address", "contractAddress", 1, true)]
        public string ContractAddress { get; set; }
    }
}
