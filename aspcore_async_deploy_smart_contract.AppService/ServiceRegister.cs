using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Nethereum.RPC.Eth.DTOs;

using aspcore_async_deploy_smart_contract.Contract.Service;
using aspcore_async_deploy_smart_contract.Contract.Repository;
using aspcore_async_deploy_smart_contract.Dal.Entities;
using aspcore_async_deploy_smart_contract.AppService.Repository;

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
            services.AddSingleton<IBECInterface<TransactionReceipt>, BECInterface.BECInterface>();

            services.AddTransient<IRepository<Certificate>, CertificateRepository>();
            services.AddTransient<IScopeService, ScopeService>();
            services.AddSingleton<ILoggerFactoryService, LoggerFactoryService>();
            return services;
        }
    }
}
