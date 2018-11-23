using System;

using Microsoft.Extensions.DependencyInjection;

using aspcore_async_deploy_smart_contract.Contract;
using System.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;

namespace aspcore_async_deploy_smart_contract.AppService
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddService(this IServiceCollection services)
        {
            services.AddHostedService<BackgroundTxIdDeployService>();
            services.AddHostedService<BackgroundReceiptPollingService>();
            services.AddSingleton<IBackgroundTaskQueue<(Guid id, Task<string> task)>, BackgroundTaskQueue<(Guid id, Task<string> task)>>();
            services.AddSingleton<IBackgroundTaskQueue<(Guid id, Task<TransactionReceipt> task)>, BackgroundTaskQueue<(Guid id, Task<TransactionReceipt> task)>>();
            services.AddTransient<IMapper, Mapper>();
            services.AddTransient<ICertificateService, CertificateService>();
            services.AddSingleton<BECInterface.BECInterface>();
            return services;
        }
    }
}
