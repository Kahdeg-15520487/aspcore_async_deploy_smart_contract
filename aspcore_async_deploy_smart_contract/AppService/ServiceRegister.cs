using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Nethereum.RPC.Eth.DTOs;

using aspcore_async_deploy_smart_contract.Contract.Service;
using aspcore_async_deploy_smart_contract.Contract.Repository;
using aspcore_async_deploy_smart_contract.Dal.Entities;
using aspcore_async_deploy_smart_contract.AppService.Repository;
using aspcore_async_deploy_smart_contract.AppService.Services;
using BECInterface;
using aspcore_async_deploy_smart_contract.Contract.DTO;

namespace aspcore_async_deploy_smart_contract.AppService
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddService(this IServiceCollection services)
        {
            services.AddHostedService<BackgroundContractDeploymentService>();
            services.AddHostedService<BackgroundReceiptQueryService>();
            //services.AddSingleton<StartupQueueUnfinishedReceiptPollingTask>();

            //todo look into the following, it seem to work properly but is it ok
            services.AddSingleton<IBackgroundTaskQueue<(string id, Task<TransactionResult> task)>, BackgroundTaskQueue<(string id, Task<TransactionResult> task)>>();
            services.AddSingleton<IBackgroundTaskQueue<(string id, Task<ContractAddress> task)>, BackgroundTaskQueue<(string id, Task<ContractAddress> task)>>();

            services.AddTransient<IMapper, Mapper>();
            services.AddSingleton<IBECInterface, BECInterface.BECInterface>();

            services.AddTransient<ICertificateService, CertificateService>();

            services.AddTransient<IRepository<Certificate>, CertificateRepository>();
            services.AddTransient<IScopeService, ScopeService>();
            services.AddSingleton<ILoggerFactoryService, LoggerFactoryService>();

            //services.AddBECInterfaceService();

            return services;
        }
    }
}
