using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using aspcore_async_deploy_smart_contract.Contract;
using aspcore_async_deploy_smart_contract.Dal;
using System.Linq;

namespace aspcore_async_deploy_smart_contract.AppService
{
    class BackgroundBECService : BackgroundService
    {
        private readonly ILogger _logger;

        private readonly IBackgroundTaskQueue<string> TaskQueue;

        private readonly BECDbContext _context;

        public BackgroundBECService(IBackgroundTaskQueue<string> taskQueue, ILoggerFactory loggerFactory, BECDbContext context)
        {
            TaskQueue = taskQueue;
            _logger = loggerFactory.CreateLogger<BackgroundBECService>();
            _context = context;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BEC service is starting");
            Console.WriteLine("BEC service is starting");
            while (!cancellationToken.IsCancellationRequested)
            {
                var workItem = await TaskQueue.DequeueAsync(cancellationToken);
                var task = workItem(cancellationToken);

                try
                {
                    var txId = await task;
                    _logger.LogInformation($"txId: {txId}");
                    Console.WriteLine($"txId: {txId}");
                    UpdateCertificateStatus(task.Id, txId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error occurred executing {nameof(workItem)}.");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine($"Error occurred executing {nameof(workItem)}.");
                }
            }

            _logger.LogInformation("BEC service is stopping");
            Console.WriteLine("BEC service is stopping");
        }

        private void UpdateCertificateStatus(int id, string txId)
        {
            var certificate = _context.Certificates.FirstOrDefault(cert => cert.TaskId == id);

            if (certificate == null)
            {
                return;
            }

            certificate.TransactionId = txId;
        }
    }
}
