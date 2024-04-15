namespace application.Services.Abstractions
{
    public interface ICacheHandler<T>
    {
        public Task<T> CacheAndGet(object dataObject);
        public Task<IEnumerable<T>> CacheAndGetRange(object dataObject);
    }
}
