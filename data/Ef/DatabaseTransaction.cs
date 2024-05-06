using Microsoft.EntityFrameworkCore.Storage;
using domain.Abstractions.Data;
using System.Transactions;

namespace data_access.Ef
{
    public class DatabaseTransaction(FileCryptDbContext dbContext) : IDatabaseTransaction
    {
        public async Task<IDbContextTransaction> BeginAsync() => await dbContext.Database.BeginTransactionAsync();

        public async Task CommitAsync(IDbContextTransaction transaction) => await transaction.CommitAsync();

        public async Task RollbackAsync(IDbContextTransaction transaction) => await transaction.RollbackAsync();

        public void Dispose(IDbContextTransaction transaction) => transaction.Dispose();
    }
}
