using System;

using Microsoft.Extensions.DependencyInjection;

using aspcore_async_deploy_smart_contract.Contract;

namespace aspcore_async_deploy_smart_contract.AppService
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddService(this IServiceCollection services)
        {
            services.AddHostedService<BackgroundBECService>();
            services.AddSingleton<IBackgroundTaskQueue<string>, BackgroundTaskQueue<string>>();
            services.AddTransient<ICertificateService, CertificateService>();
            services.AddSingleton<BECInterface.BECInterface>();
            return services;
        }
    }
}
