using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using aspcore_async_deploy_smart_contract.Contract;

namespace aspcore_async_deploy_smart_contract.AppService
{
    public class BackgroundTaskQueue<T> : IBackgroundTaskQueue<T>
    {
        private ConcurrentQueue<Func<CancellationToken, Task<T>>> _workItems;
        private SemaphoreSlim _signal;

        public int Count => _workItems.Count;

        public BackgroundTaskQueue()
        {
            _workItems = new ConcurrentQueue<Func<CancellationToken, Task<T>>>();
            _signal = new SemaphoreSlim(0);
        }

        public void QueueBackgroundWorkItem(
            Func<CancellationToken, Task<T>> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue(workItem);
            _signal.Release();
        }

        public async Task<Func<CancellationToken, Task<T>>> DequeueAsync(
            CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);

            return workItem;
        }
    }
}
