using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Repositories
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null);
        Task<T> GetFirstOrDefault(Expression<Func<T, bool>> filter,  bool tracked = false);
        Task AddAsync(T entity);
        Task AddRangeAsync(List<T> entities);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
        void Update(T entity);  
        Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
    }
}
