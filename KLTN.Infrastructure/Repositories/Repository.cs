using KLTN.Domain.Repositories;
using KLTN.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            dbSet = db.Set<T>();
        }
        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, params Expression<Func<T, object>>[] includeProperties)
        {
            return await dbSet.ToListAsync();
        }
        public async Task AddAsync(T entity)
        {
            await _db.AddAsync(entity);
        }

        public async Task AddRangeAsync(List<T> entities)
        {
            await dbSet.AddRangeAsync(entities);
        }
        public void Delete(T entity)
        {
            dbSet.Remove(entity);
        }
        public void DeleteRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if(includeProperties.Length > 0)
            {
                query = includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
            }
            return query;
        }
        public async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter, bool tracked = false, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = dbSet;
            if(!tracked)
            {
                query = dbSet.AsNoTracking();
            }  
            query = query.Where(filter);
            if (includeProperties.Length > 0)
            {
                query = includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
            }
            return await query.FirstOrDefaultAsync();
        }
        public void Update(T entity)
        {
            dbSet.Update(entity);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> filter)
        {
            return await dbSet.AnyAsync(filter);  
        }

        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false) =>
            !trackChanges
                ? _db.Set<T>().Where(expression).AsNoTracking()
                : _db.Set<T>().Where(expression);

        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false, params Expression<Func<T, object>>[] includeProperties)
        {
            var items = FindByCondition(expression, trackChanges);
            items = includeProperties.Aggregate(items, (current, includeProperty) => current.Include(includeProperty));
            return items;
        }
    }
}
