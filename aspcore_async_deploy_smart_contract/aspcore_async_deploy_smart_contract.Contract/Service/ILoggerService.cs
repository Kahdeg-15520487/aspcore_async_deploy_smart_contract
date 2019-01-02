namespace aspcore_async_deploy_smart_contract.Contract.Service
{
    public interface ILoggerService
    {
        void LogInformation(string format, params object[] objs);
        void LogDebug(string format, params object[]objs);
        void LogError(string format, params object[]objs);
    }
}
