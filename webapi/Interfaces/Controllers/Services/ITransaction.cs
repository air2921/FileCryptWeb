namespace webapi.Interfaces.Controllers.Services
{
    public interface ITransaction<T> where T : class
    {
        public Task BeginTransaction(T data, object? parameter = null);
    }
}
