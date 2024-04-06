namespace webapi.DB.Abstractions
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll(Func<IQueryable<T>, IQueryable<T>> queryModifier = null, CancellationToken cancellationToken = default);
        Task<T> GetById(int id, CancellationToken cancellationToken = default);
        Task<T> GetByFilter(Func<IQueryable<T>, IQueryable<T>>? queryModifier = null, CancellationToken cancellationToken = default);
        Task<int> Add(T entity, Func<T, int>? GetId = null, CancellationToken cancellationToken = default);
        Task AddRange(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<T> Update(T entity, CancellationToken cancellationToken = default);
        Task<T> Delete(int id, CancellationToken cancellationToken = default);
        Task<T> DeleteByFilter(Func<IQueryable<T>, IQueryable<T>> queryModifier, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> DeleteMany(IEnumerable<int> identifiers, CancellationToken cancellationToken = default);
    }
}
