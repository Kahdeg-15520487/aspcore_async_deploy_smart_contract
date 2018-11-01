using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using aspcore_async_deploy_smart_contract.Contract;
using aspcore_async_deploy_smart_contract.Contract.DTO;

namespace aspcore_async_deploy_smart_contract.WebApi
{

    [EnableCors("CorsPolicy")]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class CertificateController : Controller
    {
        private readonly ICertificateService certService;

        public CertificateController(ICertificateService certificateService)
        {
            certService = certificateService;
        }

        [HttpGet("{txId}")]
        public async Task<IActionResult> Get(string txId)
        {
            var result = await certService.QuerryContractStatus(txId);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]string hash)
        {
            var txId = await certService.DeployContract(hash);
            return Json(txId);
        }

        [HttpPost("/bulk")]
        public async Task<IActionResult> PostBulk([FromBody] BulkHashRequest bulkHashRequest)
        {
            var result = await certService.BulkDeployContract(bulkHashRequest.HashList);
            return Json(result);
        }
    }
}