namespace webapi.Interfaces.Redis
{
    public interface IRedisCache
    {
        Task<string> CacheKey(string key, Func<Task<string>> readKeyFunction);
        Task CacheData(string key, string value, TimeSpan expire);
        Task<string> GetCachedData(string key);
        Task<string> CacheDbData(string key, Func<Task<string>> read, TimeSpan expire);
        Task DeleteCache(string key);
    }
}
