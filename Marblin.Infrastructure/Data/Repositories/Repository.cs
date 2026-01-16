using Marblin.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.Extensions.Caching.Memory;

namespace Marblin.Infrastructure.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;
        private readonly IMemoryCache _cache;

        public Repository(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _dbSet = _context.Set<T>();
            _cache = cache;
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetEntityWithSpec(ISpecification<T> spec)
        {
            if (spec.IsCacheEnabled && spec.CacheKey != null)
            {
                return await _cache.GetOrCreateAsync(spec.CacheKey, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = spec.CacheDuration;
                    return ApplySpecification(spec).FirstOrDefaultAsync();
                });
            }
            return await ApplySpecification(spec).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> ListAsync(ISpecification<T> spec)
        {
            if (spec.IsCacheEnabled && spec.CacheKey != null)
            {
                return await _cache.GetOrCreateAsync(spec.CacheKey, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = spec.CacheDuration;
                    return ApplySpecification(spec).ToListAsync();
                }) ?? Enumerable.Empty<T>();
            }
            return await ApplySpecification(spec).ToListAsync();
        }

        public async Task<int> CountAsync(ISpecification<T> spec)
        {
            if (spec.IsCacheEnabled && spec.CacheKey != null)
            {
                 return await _cache.GetOrCreateAsync($"{spec.CacheKey}_count", entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = spec.CacheDuration;
                    return ApplySpecification(spec).CountAsync();
                });
            }
            return await ApplySpecification(spec).CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        private IQueryable<T> ApplySpecification(ISpecification<T> spec)
        {
            return SpecificationEvaluator<T>.GetQuery(_dbSet.AsQueryable(), spec);
        }

        // Helper methods
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) 
        { 
            return await _dbSet.Where(predicate).ToListAsync(); 
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) 
        { 
            return await _dbSet.FirstOrDefaultAsync(predicate); 
        }
        public void Add(T entity) { _dbSet.Add(entity); }
        public void Remove(T entity) { _dbSet.Remove(entity); }

    }
}
