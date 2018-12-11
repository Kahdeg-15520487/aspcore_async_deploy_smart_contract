using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace aspcore_async_deploy_smart_contract.Contract.Repository
{
    public interface IRepository<T> where T : new()
    {
        T GetById(Guid id);
        IEnumerable<T> List();
        IEnumerable<T> List(Expression<Func<T, bool>> predicate);
        void Insert(T entity);
        void Delete(T entity);
        void Update(T entity);
        int SaveChanges();
    }
}
