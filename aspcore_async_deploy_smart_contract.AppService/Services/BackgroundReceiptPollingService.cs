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
using aspcore_async_deploy_smart_contract.Contract.Service;
using aspcore_async_deploy_smart_contract.Contract.Repository;

namespace aspcore_async_deploy_smart_contract.AppService
{
    class BackgroundReceiptPollingService : BackgroundService
    {
        private readonly ILoggerService _logger;

        private readonly IBackgroundTaskQueue<(Guid id, Task<string> task)> QuerryContractTaskQueue;

        private readonly IScopeService _scopeService;

        public BackgroundReceiptPollingService(IBackgroundTaskQueue<(Guid id, Task<string> task)> querryContractTaskQueue, ILoggerFactoryService loggerFactory, IScopeService scopeService)
        {
            QuerryContractTaskQueue = querryContractTaskQueue;
            _logger = loggerFactory.CreateLogger<BackgroundReceiptPollingService>();
            _scopeService = scopeService;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BEC receipt polling service is starting");

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
                while (nextIndex < CONCURRENCY_LEVEL && nextIndex < QuerryContractTaskQueue.Count)
                {
                    var workItem = await QuerryContractTaskQueue.DequeueAsync(cancellationToken);
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
                        var receipt = await completedTask;
                        _logger.LogInformation($"txId: {receipt}");

                        FinishCertificateStatusWithReceipt(id, receipt);
                    }

                    // queue more task
                    if (nextIndex < QuerryContractTaskQueue.Count)
                    {
                        var workItem = await QuerryContractTaskQueue.DequeueAsync(cancellationToken);
                        var task = await workItem(cancellationToken);
                        currentlyRunningTasks.Add(task.task);
                        TaskTable.Add(task.task.Id, task.id);
                        nextIndex++;
                    }
                }
            }

            _logger.LogInformation("BEC service is stopping");
        }

        private void ErrorCertificateStatue(Guid id, string message)
        {
            var repo = _scopeService.GetRequiredService<IRepository<Certificate>>();
            var certificate = repo.GetById(id);
            certificate.Status = DeployStatus.ErrorInDeploy;
            certificate.Messasge = message;
            repo.Update(certificate);
            repo.SaveChanges();
            _logger.LogError("status: {0}, msg: {1}", certificate.Status, certificate.Messasge);
            _scopeService.Dispose();
        }

        private Certificate GetCertificate(Guid id)
        {
            var repo = _scopeService.GetRequiredService<IRepository<Certificate>>();
            var cert = repo.GetById(id);
            _scopeService.Dispose();
            return cert;
        }

        private void FinishCertificateStatusWithReceipt(Guid id, string receipt)
        {
            var repo = _scopeService.GetRequiredService<IRepository<Certificate>>();
            var certificate = repo.GetById(id);

            if (certificate == null)
            {
                return;
            }

            certificate.ContractAddress = receipt;
            certificate.Status = DeployStatus.DoneQuerrying;
            certificate.QuerryDone = DateTime.UtcNow; repo.Update(certificate);
            repo.Update(certificate);
            repo.SaveChanges();
            _logger.LogInformation("id: {0}, txId: {1}, hash: {2}", certificate.Id, certificate.TransactionId, certificate.Hash);
            _scopeService.Dispose();
        }
    }
}
