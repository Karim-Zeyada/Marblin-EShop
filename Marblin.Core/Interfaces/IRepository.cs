using System.Linq.Expressions;

namespace Marblin.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        
        // Specification Methods
        Task<T?> GetEntityWithSpec(ISpecification<T> spec);
        Task<IEnumerable<T>> ListAsync(ISpecification<T> spec);
        Task<int> CountAsync(ISpecification<T> spec);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        void Add(T entity);
        void Update(T entity);
        void Remove(T entity);
        

    }
}
