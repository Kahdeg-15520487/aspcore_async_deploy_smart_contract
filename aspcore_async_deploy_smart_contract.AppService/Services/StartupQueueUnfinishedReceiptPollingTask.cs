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
using aspcore_async_deploy_smart_contract.Contract.DTO;

namespace aspcore_async_deploy_smart_contract.AppService
{
    public class StartupQueueUnfinishedReceiptPollingTask : BackgroundService
    {
        private readonly ILoggerService _logger;

        private readonly IBackgroundTaskQueue<(Guid id, Task<ContractAddress> task)> QuerryContractTaskQueue;

        private readonly IScopeService _scopeService;
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly IBECInterface _bec;

        public StartupQueueUnfinishedReceiptPollingTask(IBECInterface bec, IBackgroundTaskQueue<(Guid id, Task<ContractAddress> task)> querryContractTaskQueue, ILoggerFactoryService loggerFactory, IScopeService scopeService, IServiceScopeFactory scopeFactory)
        {
            QuerryContractTaskQueue = querryContractTaskQueue;
            _logger = loggerFactory.CreateLogger<StartupQueueUnfinishedReceiptPollingTask>();
            _scopeService = scopeService;
            _bec = bec;

            _scopeFactory = scopeFactory;

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IRepository<Certificate>>();
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BEC startup queue unfinished receipt polling service is starting");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BEC startup queue unfinished receipt polling service is stopping");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IRepository<Certificate>>();
            }

            //the life time loop
            while (!cancellationToken.IsCancellationRequested)
            {
                //var repo = _scopeService.GetRequiredService<IRepository<Certificate>>();
                //foreach (var cert in repo.List(c => c.Status == DeployStatus.DoneDeploying || c.Status == DeployStatus.ErrorInQuerrying))
                //{
                //    _logger.LogInformation("{0}:org= {1}, txid= {2}, status= {3}", cert.Id.ToString("N"), cert.OrganizationId, cert.TransactionId, cert.Status);
                //    var cId = cert.Id.ToString("N");
                //    var cOrgId = cert.OrganizationId;
                //    var cTxId = cert.TransactionId;
                //    QuerryContractTaskQueue.QueueBackgroundWorkItem((ct) =>
                //    {
                //        return _bec.QuerryReceipt(cId, cOrgId, cTxId).ContinueWith(t => (cert.Id, t));
                //    });
                //}
                //_scopeService.Dispose();
                await Task.Delay(0);
                break;
            }

            //BackgroundReceiptPollingService backgroundReceiptPollingService = _scopeService.GetRequiredService<BackgroundReceiptPollingService>();

            //backgroundReceiptPollingService.StartAsync(new CancellationToken()).GetAwaiter().GetResult();

            //_scopeService.Dispose();

            _logger.LogInformation("BEC startup queue unfinished receipt polling service is ending");
        }

        private Certificate GetCertificate(Guid id)
        {
            var repo = _scopeService.GetRequiredService<IRepository<Certificate>>();
            var cert = repo.GetById(id);
            _scopeService.Dispose();
            return cert;
        }
    }
}