using System.Linq.Expressions;

namespace HedgeR.Shared.Entities
{
    public interface IRepository<T> where T : IEntity
    {
        Task CreateAsync(T entity);

        Task<IReadOnlyCollection<T>> GetAllAsync();

        Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter);

        Task<T> GetAsync(int id);

        Task<T> GetAsync(Expression<Func<T, bool>> filter);

        Task RemoveAsync(int id);

        Task UpdateAsync(T entity);
    }
}