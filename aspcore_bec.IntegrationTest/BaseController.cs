using aspcore_async_deploy_smart_contract.Startup;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace aspcore_bec.IntegrationTest
{
    public class BaseController : IDisposable
    {
        public string BASE_URL { get; private set; }
        public Uri BASE_URI { get; private set; }

        public IWebHost _webhost { get; private set; }

        public BaseController(int port)
        {
            _webhost = null;
            Init(port);
        }

        public BaseController()
        {
            Init(49911);
        }

        protected void Init(int port, string connstr = null)
        {
            var assemblyName = typeof(aspcore_async_deploy_smart_contract.Startup.Startup).GetTypeInfo().Assembly.FullName;
            var projectDir = System.IO.Directory.GetCurrentDirectory();
            BASE_URL = $"http://localhost:{port}";
            BASE_URI = new Uri(BASE_URL);
            var configuration = new ConfigurationBuilder()
                                    .SetBasePath(projectDir)
                                    .AddJsonFile("appsettings.json")
                                    .AddJsonFile("appsettings.Development.json")
                                    .AddJsonFile("appsettings.DevelopmentTest.json")
                                    .Build();
            TestConnectionString testConnectionString =
                new TestConnectionString(string.IsNullOrEmpty(connstr) ?
                configuration["ConnectionStrings:DefaultConnection"] :
                connstr);
            _webhost = WebHost.CreateDefaultBuilder(null)
                              .ConfigureServices(ss => ss.AddSingleton(testConnectionString))
                              .UseStartup(assemblyName)
                              .UseEnvironment("DevelopmentTest")
                              .UseConfiguration(configuration)
                              .UseKestrel()
                              .UseUrls(BASE_URL)
                              .Build();

            _webhost.Start();
        }

        public void Dispose()
        {
            _webhost?.Dispose();
        }
    }
}
