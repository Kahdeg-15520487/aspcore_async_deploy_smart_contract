using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace aspcore_async_deploy_smart_contract.Contract
{
    public interface IBackgroundTaskQueue<T>
    {
        void QueueBackgroundWorkItem(Func<CancellationToken, Task<T>> workItem);

        Task<Func<CancellationToken, Task<T>>> DequeueAsync(CancellationToken cancellationToken);
    }
}
