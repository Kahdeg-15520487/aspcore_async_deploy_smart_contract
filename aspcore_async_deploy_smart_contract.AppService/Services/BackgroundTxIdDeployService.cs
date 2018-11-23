using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using Nethereum.RPC.Eth.DTOs;

using aspcore_async_deploy_smart_contract.Contract;
using aspcore_async_deploy_smart_contract.Dal;
using aspcore_async_deploy_smart_contract.Dal.Entities;

namespace aspcore_async_deploy_smart_contract.AppService
{
    class BackgroundTxIdDeployService : BackgroundService
    {
        private readonly ILogger _logger;

        private readonly IBackgroundTaskQueue<(Guid id, Task<string> task)> DeployContractTaskQueue;
        private readonly IBackgroundTaskQueue<(Guid id, Task<TransactionReceipt> task)> QuerryContractTaskQueue;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly BECInterface.BECInterface _bec;

        public BackgroundTxIdDeployService(BECInterface.BECInterface bec, IBackgroundTaskQueue<(Guid id, Task<string> task)> deployContractTaskQueue, IBackgroundTaskQueue<(Guid id, Task<TransactionReceipt> task)> querryContractTaskQueue, ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory)
        {
            _bec = bec;
            DeployContractTaskQueue = deployContractTaskQueue;
            QuerryContractTaskQueue = querryContractTaskQueue;
            _logger = loggerFactory.CreateLogger<BackgroundTxIdDeployService>();
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BEC service is starting");

            //the life time loop
            while (!cancellationToken.IsCancellationRequested)
            {
                //number of task that run parallel
                int CONCURRENCY_LEVEL = 5;
                //book keeping variable
                int nextIndex = 0;
                List<Task<string>> currentlyRunningTasks = new List<Task<string>>();
                Dictionary<int, Guid> TaskTable = new Dictionary<int, Guid>();

                //setup parallel task to run
                while (nextIndex < CONCURRENCY_LEVEL && nextIndex < DeployContractTaskQueue.Count)
                {
                    var workItem = await DeployContractTaskQueue.DequeueAsync(cancellationToken);
                    var task = await workItem(cancellationToken);
                    currentlyRunningTasks.Add(task.task);
                    TaskTable.Add(task.task.Id, task.id);
                    nextIndex++;
                }

                //the execution loop
                while (currentlyRunningTasks.Count > 0 || cancellationToken.IsCancellationRequested)
                {
                    //get the first completed task in the task list
                    var completedTask = await Task.WhenAny(currentlyRunningTasks);
                    currentlyRunningTasks.Remove(completedTask);
                    var id = TaskTable[completedTask.Id];
                    //exception handling
                    if (completedTask.IsFaulted)
                    {
                        _logger.LogError("faulted task's id: {0}", completedTask.Id);
                        _logger.LogError("faulted task's hash: {0}", GetCertificate(id));
                        _logger.LogError("Exception: {0}", string.Join(Environment.NewLine, completedTask.Exception.InnerExceptions.Select(ex => $"{ex.GetType().Name}{ex.Message}")));
                        //todo report errored hash
                        //maybe try it again later?
                        ErrorCertificateStatue(id, string.Join(Environment.NewLine, completedTask.Exception.InnerExceptions.Select(ex => $"{ex.GetType().Name}{ex.Message}")));
                    }
                    else
                    {
                        //the querry result is here
                        var txId = await completedTask;
                        _logger.LogInformation($"txId: {txId}");
                        FinishCertificateStatus(id, txId);

                        //queue a querry task for this transaction id?
                        QuerryContractTaskQueue.QueueBackgroundWorkItem((ct) =>
                        {
                            return _bec.QuerryReceipt(txId).ContinueWith(t => (id, t));
                        });
                    }

                    // queue more task
                    if (nextIndex < DeployContractTaskQueue.Count)
                    {
                        var workItem = await DeployContractTaskQueue.DequeueAsync(cancellationToken);
                        var task = await workItem(cancellationToken);
                        currentlyRunningTasks.Add(task.task);
                        TaskTable.Add(task.task.Id, task.id);
                        nextIndex++;
                    }

                    //try
                    //{
                    //    var txId = await completedTask;
                    //    _logger.LogInformation($"txId: {txId}");
                    //    Console.WriteLine($"txId: {txId}");
                    //    UpdateCertificateStatus(task.Id, txId);
                    //}
                    //catch (Exception ex)
                    //{
                    //    _logger.LogError(ex, $"Error occurred executing {nameof(workItem)}.");
                    //    Console.WriteLine(ex.Message);
                    //    Console.WriteLine($"Error occurred executing {nameof(workItem)}.");
                    //}
                }
            }

            _logger.LogInformation("BEC service is stopping");
        }

        private void ErrorCertificateStatue(Guid id, string message)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                using (var _context = scope.ServiceProvider.GetRequiredService<BECDbContext>())
                {
                    var certificate = _context.Certificates.FirstOrDefault(cert => cert.Id.Equals(id));

                    certificate.Status = DeployStatus.ErrorInDeploy;
                    certificate.Messasge = message;
                    _context.SaveChanges();
                    _logger.LogError("status: {0}, msg: {1}", certificate.Status, certificate.Messasge);
                }
            }
        }

        private Certificate GetCertificate(Guid id)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                using (var _context = scope.ServiceProvider.GetRequiredService<BECDbContext>())
                {
                    var certificate = _context.Certificates.FirstOrDefault(cert => cert.Id.Equals(id));

                    return certificate;
                }
            }
        }

        private void FinishCertificateStatus(Guid id, string txId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                using (var _context = scope.ServiceProvider.GetRequiredService<BECDbContext>())
                {
                    var certificate = _context.Certificates.FirstOrDefault(cert => cert.Id.Equals(id));

                    if (certificate == null)
                    {
                        return;
                    }

                    certificate.TransactionId = txId;
                    certificate.Status = DeployStatus.DoneDeploying;
                    certificate.DeployDone = DateTime.UtcNow;
                    _context.SaveChanges();
                    _logger.LogInformation("id: {0}, txId: {1}", certificate.Id, txId);
                }
            }
        }
    }
}
