using Ardalis.Specification;

namespace domain.Abstractions.Data
{
    public interface IRepository<T> where T : class
    {
        int GetCount(ISpecification<T>? specification, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAll(ISpecification<T>? specification = null, CancellationToken cancellationToken = default);
        Task<T> GetById(int id, CancellationToken cancellationToken = default);
        Task<T> GetByFilter(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<int> Add(T entity, Func<T, int>? GetId = null, CancellationToken cancellationToken = default);
        Task AddRange(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<T> Update(T entity, CancellationToken cancellationToken = default);
        Task<T> Delete(int id, CancellationToken cancellationToken = default);
        Task<T> DeleteByFilter(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> DeleteMany(IEnumerable<int> identifiers, CancellationToken cancellationToken = default);
    }
}
