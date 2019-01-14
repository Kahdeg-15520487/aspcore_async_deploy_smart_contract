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
    public class BackgroundContractDeploymentService : BackgroundService
    {
        private readonly ILogger _logger;

        private readonly IBackgroundTaskQueue<(string id, Task<TransactionResult> task)> DeployContractTaskQueue;
        private readonly IBackgroundTaskQueue<(string id, Task<ContractAddress> task)> QuerryContractTaskQueue;

        private readonly IScopeService _scopeService;
        public IServiceProvider Services { get; }


        public BackgroundContractDeploymentService(IServiceProvider services, IBackgroundTaskQueue<(string id, Task<TransactionResult> task)> deployContractTaskQueue, IBackgroundTaskQueue<(string id, Task<ContractAddress> task)> querryContractTaskQueue, ILoggerFactory loggerFactory, IScopeService scopeService)
        {
            Services = services;
            DeployContractTaskQueue = deployContractTaskQueue;
            QuerryContractTaskQueue = querryContractTaskQueue;
            _logger = loggerFactory.CreateLogger<BackgroundContractDeploymentService>();
            _scopeService = scopeService;

        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BEC transaction deploy service is starting");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BEC transaction deploy service is stopping");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //the life time loop
            while (!cancellationToken.IsCancellationRequested) {
                var workItem = await DeployContractTaskQueue.DequeueAsync(cancellationToken);
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
                        //          task.Exception.ToPrettyString()));
                        SetCertificateStatus(id, DeployStatus.Retrying);
                        var cert = GetCertificate(id);
                        using (var scope = Services.CreateScope()) {
                            var bec =
                                scope.ServiceProvider
                                    .GetRequiredService<IBECInterface>();
                            DeployContractTaskQueue.QueueBackgroundWorkItem((ct) => {
                                return bec.DeployContract(HardCodeData.accountAddr, HardCodeData.password, id.ToString(), cert.OrganizationId, cert.Hash).ContinueWith(txid => (id.ToString(), txid));
                            });
                        }

                    } else {
                        //the query result is here
                        var transactionResult = await task;
                        if (string.IsNullOrEmpty(transactionResult.TxId)) {
                            ErrorCertificateStatue(transactionResult.CertificateId, "no txid");

                            _logger.LogError("No transaction id has returned properly");
                        } else {
                            _logger.LogInformation($"Transaction Result: {transactionResult}");
                            FinishCertificateStatus(transactionResult.CertificateId, transactionResult.TxId);

                            var cert = GetCertificate(transactionResult.CertificateId);
                            using (var scope = Services.CreateScope()) {
                                var bec =
                                    scope.ServiceProvider
                                        .GetRequiredService<IBECInterface>();

                                //queue a querry task for this transaction id?
                                QuerryContractTaskQueue.QueueBackgroundWorkItem((ct) => {
                                    return bec.QuerryReceipt(cert.Id.ToString(), cert.OrganizationId,
                                        transactionResult.TxId).ContinueWith(t => (id, t));
                                });
                            }

                        }
                    }
                } catch (Exception ex) {
                    _logger.LogError(
                        $"Error occurred executing {nameof(workItem)}.", ex);
                }
            }
        }

        private void ErrorCertificateStatue(string certId, string message)
        {
            var repo = _scopeService.GetRequiredService<IRepository<Certificate>>();
            var certificate = repo.GetById(Guid.Parse(certId));
            certificate.Status = DeployStatus.ErrorInDeploy;
            certificate.Messasge = message;
            repo.Update(certificate);
            repo.SaveChanges();
            _logger.LogDebug("status: {0}, msg: {1}", certificate.Status, certificate.Messasge);
            _scopeService.Dispose();
        }

        private Certificate GetCertificate(string certId)
        {
            var repo = _scopeService.GetRequiredService<IRepository<Certificate>>();
            var cert = repo.GetById(Guid.Parse(certId));
            _scopeService.Dispose();
            return cert;
        }

        private void SetCertificateStatus(string certId, DeployStatus deployStatus)
        {
            var repo = _scopeService.GetRequiredService<IRepository<Certificate>>();
            var certificate = repo.GetById(Guid.Parse(certId));
            certificate.Status = deployStatus;
            repo.Update(certificate);
            repo.SaveChanges();
            _logger.LogInformation("id: {0}, deployStatus: {1}", certificate.Id, deployStatus);
            _scopeService.Dispose();
        }

        private void FinishCertificateStatus(string certId, string txId)
        {
            var repo = _scopeService.GetRequiredService<IRepository<Certificate>>();
            var certificate = repo.GetById(Guid.Parse(certId));
            certificate.TransactionId = txId;
            certificate.Status = DeployStatus.Querrying;
            certificate.DeployDone = DateTime.UtcNow;
            repo.Update(certificate);
            repo.SaveChanges();
            _logger.LogInformation("id: {0}, txId: {1}", certificate.Id, txId);
            _scopeService.Dispose();
        }
    }
}
