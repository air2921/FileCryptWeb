namespace application.Helper_Services
{
    public interface ITransaction<T>
    {
        public Task CreateTransaction(T data, object? parameter = null);
    }
}
