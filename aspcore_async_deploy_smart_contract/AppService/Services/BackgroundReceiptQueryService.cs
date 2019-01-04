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
using Microsoft.Extensions.Hosting;

namespace aspcore_async_deploy_smart_contract.AppService.Services
{
    public class BackgroundReceiptQueryService : BackgroundService
    {
        private readonly ILoggerService _logger;

        private readonly IBackgroundTaskQueue<(Guid id, Task<ContractAddress> task)> QuerryContractTaskQueue;

        private readonly IScopeService _scopeService;

        public BackgroundReceiptQueryService(IBackgroundTaskQueue<(Guid id, Task<ContractAddress> task)> querryContractTaskQueue, ILoggerFactoryService loggerFactory, IScopeService scopeService)
        {
            QuerryContractTaskQueue = querryContractTaskQueue;
            _logger = loggerFactory.CreateLogger<BackgroundReceiptQueryService>();
            _scopeService = scopeService;
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
            try
            {
                //the life time loop
                while (!cancellationToken.IsCancellationRequested)
                {
                    var workItem = await QuerryContractTaskQueue.DequeueAsync(cancellationToken);
                    try
                    {
                        var (id, task) = await workItem(cancellationToken);
                        if (task.IsFaulted)
                        {
                            _logger.LogError("faulted task's id: {0}", task.Id);
                            //_logger.LogError("Exception: {0}",
                            //    string.Join(Environment.NewLine,
                            //        task.Exception.InnerExceptions.Select(ex => $"{ex.GetType().Name}:{ex.Message},")));
                            _logger.LogError("Exception: {0}", task.Exception.ToString());
                            //todo report errored hash
                            //maybe try it again later?
                            ErrorCertificateStatue(id,task.ToString());
                        }
                        else
                        {
                            //the query result is here
                            var contractAddress = await task;
                            _logger.LogInformation($"txId: {contractAddress}");

                            FinishCertificateStatusWithContractAddress(contractAddress.CertificateId, contractAddress.ContractAddr);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            $"Error occurred executing {nameof(workItem)}.", ex);
                    }


                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            _logger.LogInformation("BEC receipt polling service is ending");
        }

        private void ErrorCertificateStatue(string certId, string message)
        {
            ErrorCertificateStatue(Guid.Parse(certId), message);
        }

        private void ErrorCertificateStatue(Guid certId, string message)
        {
            var repo = _scopeService.GetRequiredService<IRepository<Certificate>>();
            var certificate = repo.GetById(certId);
            certificate.Status = DeployStatus.ErrorInQuerrying;
            certificate.Messasge = message;
            repo.Update(certificate);
            repo.SaveChanges();
            _logger.LogError("status: {0}, msg: {1}", certificate.Status, certificate.Messasge);
            _scopeService.Dispose();
        }

        private Certificate GetCertificate(string certId)
        {
            var repo = _scopeService.GetRequiredService<IRepository<Certificate>>();
            var cert = repo.GetById(Guid.Parse(certId));
            _scopeService.Dispose();
            return cert;
        }

        private void FinishCertificateStatusWithContractAddress(string certId, string receipt)
        {
            var repo = _scopeService.GetRequiredService<IRepository<Certificate>>();
            var certificate = repo.GetById(Guid.Parse(certId));

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
