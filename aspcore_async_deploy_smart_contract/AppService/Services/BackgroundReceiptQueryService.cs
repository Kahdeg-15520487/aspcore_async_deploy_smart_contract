using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using aspcore_async_deploy_smart_contract.Contract.DTO;
using aspcore_async_deploy_smart_contract.Contract.Repository;
using aspcore_async_deploy_smart_contract.Contract.Service;
using aspcore_async_deploy_smart_contract.Dal.Entities;
using aspcore_async_deploy_smart_contract.Helper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace aspcore_async_deploy_smart_contract.AppService.Services
{
    public class BackgroundReceiptQueryService : BackgroundService
    {
        private readonly ILogger _logger;

        private readonly IBackgroundTaskQueue<(string id, Task<ContractAddress> task)> QueryContractTaskQueue;

        private readonly IServiceScopeFactory _scopeFactory;

        public BackgroundReceiptQueryService(IBackgroundTaskQueue<(string id, Task<ContractAddress> task)> querryContractTaskQueue, ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory)
        {
            QueryContractTaskQueue = querryContractTaskQueue;
            _logger = loggerFactory.CreateLogger<BackgroundReceiptQueryService>();
            _scopeFactory = scopeFactory;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BEC receipt polling service is starting");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BEC receipt polling service is stopping");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try {
                //the life time loop
                while (!cancellationToken.IsCancellationRequested) {
                    var workItem = await QueryContractTaskQueue.DequeueAsync(cancellationToken);
                    try {
                        var (id, task) = await workItem(cancellationToken);
                        if (task.IsFaulted) {
                            _logger.LogError("faulted task's id: {0}", task.Id);
                            _logger.LogError("Exception: {0}",
                                string.Join(Environment.NewLine,
                                    task.Exception.ToPrettyString()));

                            //todo report errored hash
                            //maybe try it again later?
                            //ErrorCertificateStatue(id,
                            //    string.Join(Environment.NewLine,
                            //        task.Exception.ToPrettyString()));


                        } else {
                            //the query result is here
                            var contractAddress = await task;
                            _logger.LogInformation($"txId: {contractAddress}");

                            FinishCertificateStatusWithContractAddress(contractAddress.CertificateId, contractAddress.ContractAddr);
                        }
                    } catch (Exception ex) {
                        _logger.LogError(
                            $"Error occurred executing {nameof(workItem)}.", ex);
                    }


                }
            } catch (Exception ex) {
                _logger.LogError(ex.ToString());
            }
            _logger.LogInformation("BEC receipt polling service is ending");
        }

        private void ErrorCertificateStatue(string certId, string message)
        {
            using (var scope = _scopeFactory.CreateScope()) {
                var repo = scope.ServiceProvider.GetRequiredService<IRepository<Certificate>>();
                var certificate = repo.GetCertificate(Guid.Parse(certId));
                certificate.SmartContractStatus = DeployStatus.ErrorInQuerying;
                certificate.Messasge = message;
                repo.Update(certificate);
                repo.Save();
                _logger.LogError("status: {0}, msg: {1}", certificate.SmartContractStatus, certificate.Messasge);
            }
        }

        private Certificate GetCertificate(string certId)
        {
            using (var scope = _scopeFactory.CreateScope()) {
                var repo = scope.ServiceProvider.GetRequiredService<IRepository<Certificate>>();
                var cert = repo.GetCertificate(Guid.Parse(certId));
                return cert;
            }
        }

        private void FinishCertificateStatusWithContractAddress(string certId, string receipt)
        {
            using (var scope = _scopeFactory.CreateScope()) {
                var repo = scope.ServiceProvider.GetRequiredService<IRepository<Certificate>>();
                var certificate = repo.GetCertificate(Guid.Parse(certId));

                if (certificate == null) {
                    return;
                }

                certificate.SmartContractAddress = receipt;
                certificate.SmartContractStatus = DeployStatus.DoneQuerying;
                certificate.QueryDone = DateTime.UtcNow; repo.Update(certificate);
                repo.Update(certificate);
                repo.Save();
                _logger.LogInformation("id: {0}, txId: {1}, hash: {2}", certificate.Id, certificate.TransactionId, certificate.Hash);
            }
        }
    }
}
