namespace webapi.DB.Abstractions
{
    public interface IDatabaseTransaction : IAsyncDisposable
    {
        public Task CommitAsync();
        public Task RollbackAsync();
    }
}
