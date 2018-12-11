using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace aspcore_async_deploy_smart_contract.Contract.Service
{
    public interface IScopeService
    {
        T GetRequiredService<T>();
        void Dispose();
    }
}
