using Microsoft.EntityFrameworkCore.Storage;
using webapi.Interfaces;

namespace webapi.DB
{
    public class DatabaseTransaction : IDatabaseTransaction
    {
        private readonly IDbContextTransaction _transaction;

        public DatabaseTransaction(FileCryptDbContext dbContext)
        {
            _transaction = dbContext.Database.BeginTransaction();
        }

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
        }
    }
}
