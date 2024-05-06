using Microsoft.EntityFrameworkCore.Storage;

namespace domain.Abstractions.Data
{
    public interface IDatabaseTransaction
    {
        public Task<IDbContextTransaction> BeginAsync();
        public Task CommitAsync(IDbContextTransaction transaction);
        public Task RollbackAsync(IDbContextTransaction transaction);
        public void Dispose(IDbContextTransaction transaction);
    }
}
