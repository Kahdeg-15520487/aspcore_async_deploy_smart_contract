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
        private readonly IBackgroundTaskQueue<(string id, Task<ContractAddress> task)> QueryContractTaskQueue;

        private readonly IServiceScopeFactory _scopeFactory;
        public IServiceProvider Services { get; }


        public BackgroundContractDeploymentService(IServiceProvider services, IBackgroundTaskQueue<(string id, Task<TransactionResult> task)> deployContractTaskQueue, IBackgroundTaskQueue<(string id, Task<ContractAddress> task)> querryContractTaskQueue, ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory)
        {
            Services = services;
            DeployContractTaskQueue = deployContractTaskQueue;
            QueryContractTaskQueue = querryContractTaskQueue;
            _logger = loggerFactory.CreateLogger<BackgroundContractDeploymentService>();
            _scopeFactory = scopeFactory;
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
                        var msg = string.Join(Environment.NewLine,
                                task.Exception.ToPrettyString());
                        _logger.LogError("faulted task's id: {0}", task.Id);
                        _logger.LogError("Exception: {0}", msg);

                        //todo report errored hash
                        //maybe try it again later?
                        //ErrorCertificateStatue(id,
                        //    string.Join(Environment.NewLine,
                        //          task.Exception.ToPrettyString()));
                        SetCertificateStatus(id, DeployStatus.Retrying);
                        SetCertificateMessage(id, msg);
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
                                QueryContractTaskQueue.QueueBackgroundWorkItem((ct) => {
                                    return bec.QueryReceipt(cert.Id.ToString(), cert.OrganizationId,
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
            using (var scope = _scopeFactory.CreateScope()) {
                var repo = scope.ServiceProvider.GetRequiredService<IRepository<Certificate>>();
                var certificate = repo.GetCertificate(Guid.Parse(certId));
                certificate.SmartContractStatus = DeployStatus.ErrorInDeploy;
                certificate.Messasge = message;
                repo.Update(certificate);
                repo.Save();
                _logger.LogDebug("status: {0}, msg: {1}", certificate.SmartContractStatus, certificate.Messasge);
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

        private void SetCertificateStatus(string certId, DeployStatus deployStatus)
        {
            using (var scope = _scopeFactory.CreateScope()) {
                var repo = scope.ServiceProvider.GetRequiredService<IRepository<Certificate>>();
                var certificate = repo.GetCertificate(Guid.Parse(certId));
                certificate.SmartContractStatus = deployStatus;
                repo.Update(certificate);
                repo.Save();
                _logger.LogInformation("id: {0}, deployStatus: {1}", certificate.Id, deployStatus);
            }
        }

        private void SetCertificateMessage(string certId, string msg)
        {
            using (var scope = _scopeFactory.CreateScope()) {
                var repo = scope.ServiceProvider.GetRequiredService<IRepository<Certificate>>();
                var certificate = repo.GetCertificate(Guid.Parse(certId));
                certificate.Messasge = msg;
                repo.Update(certificate);
                repo.Save();
                _logger.LogInformation("id: {0}, msg: {1}", certificate.Id, msg);
            }
        }

        private void FinishCertificateStatus(string certId, string txId)
        {
            using (var scope = _scopeFactory.CreateScope()) {
                var repo = scope.ServiceProvider.GetRequiredService<IRepository<Certificate>>();
                var certificate = repo.GetCertificate(Guid.Parse(certId));
                certificate.TransactionId = txId;
                certificate.SmartContractStatus = DeployStatus.Querying;
                certificate.DeployDone = DateTime.UtcNow;
                repo.Update(certificate);
                repo.Save();
                _logger.LogInformation("id: {0}, txId: {1}", certificate.Id, txId);
            }
        }
    }
}
