namespace webapi.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll(Func<IQueryable<T>, IQueryable<T>> queryModifier = null);
        Task<T> GetById (int id);
        Task<T> GetByFilter(Func<IQueryable<T>, IQueryable<T>>? queryModifier = null);
        Task<int> Add (T entity, Func<T, int>? GetId = null);
        Task AddRange(IEnumerable<T> entities);
        Task Update(T entity);
        Task Delete(int id);
        Task DeleteByFilter(Func<IQueryable<T>, IQueryable<T>> queryModifier);
        Task DeleteMany(IEnumerable<int> identifiers);
    }
}
