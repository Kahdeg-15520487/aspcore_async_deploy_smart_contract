using Microsoft.Extensions.Logging;

namespace aspcore_async_deploy_smart_contract.Contract.Service
{
    public interface ILoggerFactoryService
    {
        ILoggerService CreateLogger<T>();
    }
}
