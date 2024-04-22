namespace domain.Abstractions.Data
{
    public interface IRedisCache
    {
        Task CacheData(string key, object value, TimeSpan expire);
        Task<string> GetCachedData(string key);
        Task DeleteCache(string key);
        Task DeteteCacheByKeyPattern(string key);
        Task DeleteRedisCache<T>(IEnumerable<T> data, string prefix, Func<T, int> getUserId) where T : class;
    }
}
