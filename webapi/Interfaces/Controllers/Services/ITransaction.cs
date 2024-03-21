namespace webapi.Interfaces.Controllers.Services
{
    public interface ITransaction<T>
    {
        public Task CreateTransaction(T data, object? parameter = null);
    }
}
