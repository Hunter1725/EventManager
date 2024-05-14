using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repositories
{
    public interface IRepositoryBase<T> where T : class
    {
        IEnumerable<T> FindAll(bool trackChanges);
        IEnumerable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges);
        void Add(T entity);
        void Delete(T entity);
        void Update(T entity);
        Task<T> GetById(int id);
    }
}
