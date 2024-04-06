namespace webapi.Services.Core.Data_Handlers
{
    public interface ICacheHandler<T>
    {
        public Task<T> CacheAndGet(object dataObject);
        public Task<IEnumerable<T>> CacheAndGetRange(object dataObject);
    }
}
