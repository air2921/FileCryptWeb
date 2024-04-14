namespace domain.Abstractions.Data
{
    public interface IDatabaseTransaction : IAsyncDisposable
    {
        public Task CommitAsync();
        public Task RollbackAsync();
    }
}
