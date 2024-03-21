namespace webapi.Interfaces
{
    public interface IDatabaseTransaction : IAsyncDisposable
    {
        public Task CommitAsync();
        public Task RollbackAsync();
    }
}
