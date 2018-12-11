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
    [Route("api/[controller]")]
    public class CertificateController : Controller
    {
        private readonly ICertificateService certService;

        public CertificateController(ICertificateService certificateService)
        {
            certService = certificateService;
        }

        [HttpGet("{txId}")]
        public CertificateDTO Get(string txId)
        {
            return certService.GetCertificate(txId);
        }

        [HttpGet]
        public IEnumerable<CertificateDTO> Gets()
        {
            return certService.GetCertificates();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]string hash)
        {
            //var txId = await certService.DeployContract(hash);
            //return Json(txId);
            return Ok();
        }
    }
}