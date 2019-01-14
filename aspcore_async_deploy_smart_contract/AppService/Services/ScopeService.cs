using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using aspcore_async_deploy_smart_contract.Contract.Service;

namespace aspcore_async_deploy_smart_contract.AppService
{
    /// <summary>
    /// helper class so that singleton/hosted service can access scoped/transient service
    /// </summary>
    public class ScopeService : IScopeService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private List<IServiceScope> createdScopes;

        public ScopeService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            createdScopes = new List<IServiceScope>();
        }

        public T GetRequiredService<T>()
        {
            var scope = _scopeFactory.CreateScope();
            createdScopes.Add(scope);
            return scope.ServiceProvider.GetRequiredService<T>();
        }

        public void Dispose()
        {
            foreach (var scope in createdScopes)
            {
                scope.Dispose();
            }
        }
    }
}
