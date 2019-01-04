using Microsoft.Extensions.Logging;

using aspcore_async_deploy_smart_contract.Contract.Service;

namespace aspcore_async_deploy_smart_contract.AppService
{
    class LoggerFactoryService : ILoggerFactoryService
    {
        private readonly ILoggerFactory loggerFactory;
        public LoggerFactoryService(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public ILoggerService CreateLogger<T>()
        {
            return new LoggerService(loggerFactory.CreateLogger<T>());
        }
    }
}
