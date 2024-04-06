using Microsoft.EntityFrameworkCore.Storage;
using webapi.DB.Abstractions;

namespace webapi.DB.Ef
{
    public class DatabaseTransaction(FileCryptDbContext dbContext) : IDatabaseTransaction
    {
        private readonly IDbContextTransaction _transaction = dbContext.Database.BeginTransaction();

        public async Task CommitAsync()
        {
            await _transaction.CommitAsync();
        }

        public async Task RollbackAsync()
        {
            await _transaction.RollbackAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _transaction.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}
