using Microsoft.Extensions.Logging;

using aspcore_async_deploy_smart_contract.Contract.Service;

namespace aspcore_async_deploy_smart_contract.AppService
{
    class LoggerService : ILoggerService
    {
        private readonly ILogger logger;
        public LoggerService(ILogger logger)
        {
            this.logger = logger;
        }

        public void LogDebug(string format, params object[] objs)
        {
            logger.LogDebug(format, objs);
        }

        public void LogError(string format, params object[] objs)
        {
            logger.LogError(format, objs);
        }

        public void LogInformation(string format, params object[] objs)
        {
            logger.LogInformation(format, objs);
        }
    }
}
