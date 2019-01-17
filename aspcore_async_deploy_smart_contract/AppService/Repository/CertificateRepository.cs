using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Microsoft.EntityFrameworkCore;

using aspcore_async_deploy_smart_contract.Contract.Repository;
using aspcore_async_deploy_smart_contract.Dal;
using aspcore_async_deploy_smart_contract.Dal.Entities;

namespace aspcore_async_deploy_smart_contract.AppService.Repository
{
    class CertificateRepository : IRepository<Certificate>
    {
        private readonly BECDbContext _dbContext;

        public CertificateRepository(BECDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Certificate GetCertificate(Guid id)
        {
            return _dbContext.Certificates.Find(id);
        }

        public IEnumerable<Certificate> GetListCertificates()
        {
            return _dbContext.Certificates.AsEnumerable();
        }

        public IEnumerable<Certificate> List(Expression<Func<Certificate, bool>> predicate)
        {
            return _dbContext.Certificates
               .Where(predicate)
               .AsEnumerable();
        }

        public void Insert(Certificate entity)
        {
            _dbContext.Certificates.Add(entity);
        }

        public void Update(Certificate entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(Certificate entity)
        {
            _dbContext.Certificates.Remove(entity);
        }

        public int Save()
        {
            return _dbContext.SaveChanges();
        }
    }
}
