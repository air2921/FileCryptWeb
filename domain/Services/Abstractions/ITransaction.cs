namespace domain.Services.Abstractions
{
    public interface ITransaction<T>
    {
        public Task CreateTransaction(T data, object? parameter = null);
    }
}
