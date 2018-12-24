using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using aspcore_async_deploy_smart_contract.Contract;
using aspcore_async_deploy_smart_contract.Contract.DTO;
using aspcore_async_deploy_smart_contract.Contract.Service;

namespace aspcore_async_deploy_smart_contract.WebApi
{
    [EnableCors("CorsPolicy")]
    [Produces("application/json")]
    [Route("api/certificate/bulk")]
    public class BulkCertificateController : Controller
    {
        private readonly ICertificateService certService;

        public BulkCertificateController(ICertificateService certificateService)
        {
            certService = certificateService;
        }

        //[HttpPost]
        //public async Task<IActionResult> PostBulk([FromBody] BulkHashRequest bulkHashRequest)
        //{
        //    var result = await certService.BulkDeployContract(bulkHashRequest.HashList);
        //    return Json(result);
        //}

        [HttpPost]
        public async Task<IActionResult> PostBulk([FromBody] BulkHashRequest bulkHashRequest)
        {
            //var result = await certService.BulkDeployContract(bulkHashRequest.HashList);
            //return Json(result);
            certService.BulkDeployContractWithBackgroundTask("bobo.inc", bulkHashRequest.HashList);
            return Ok();
        }
    }
}
