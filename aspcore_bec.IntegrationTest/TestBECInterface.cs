using aspcore_async_deploy_smart_contract.Contract.DTO;
using aspcore_async_deploy_smart_contract.Dal.Entities;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;

namespace aspcore_bec.IntegrationTest
{
    public class TestBECInterface
    {
        BaseController controller;
        private readonly ITestOutputHelper _output;

        public TestBECInterface(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData(49999)]
        public void GetAllCertificate(int port)
        {
            controller = new BaseController(port);

            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = controller.BASE_URI;

                HttpResponseMessage result = client.GetAsync("api/certificate").GetAwaiter().GetResult();
                var content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                _output.WriteLine(content);

                Assert.True(true);
            }

        }

        [Theory]
        [InlineData("", 49998)]
        public void PostCertificate(string hash, int port)
        {
            controller = new BaseController(port);

            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = controller.BASE_URI;

                //clear database 
                {
                    HttpResponseMessage result = client.DeleteAsync("api/certificate")
                                                .GetAwaiter().GetResult();
                    _output.WriteLine(result.IsSuccessStatusCode.ToString());
                }

                {
                    HttpResponseMessage result = client.PostAsJsonAsync("api/certificate", new HashRequest { OrganizationId = "bobo.inc", Hash = hash })
                                                .GetAwaiter().GetResult();
                    var content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _output.WriteLine(content);
                    _output.WriteLine(result.StatusCode.ToString());
                    _output.WriteLine(result.ReasonPhrase);

                    Assert.True(result.IsSuccessStatusCode);
                }

                {
                    bool isSuccess = false;
                    //todo need time out
                    do {
                        HttpResponseMessage result = client.GetAsync("api/certificate").GetAwaiter().GetResult();
                        var content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        _output.WriteLine(content);
                        CertificateDTO[] certs = JsonConvert.DeserializeObject<CertificateDTO[]>(content);
                        if (certs.Length > 0) {
                            var cert = certs[0];
                            isSuccess = cert.DeployStatus == DeployStatus.DoneQuerrying.ToString();
                            _output.WriteLine(cert.DeployStatus + "  " + isSuccess);
                        }
                    } while (!isSuccess);
                }
            }

        }
    }
}
