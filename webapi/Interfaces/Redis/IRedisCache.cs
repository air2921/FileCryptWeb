using webapi.Models;

namespace webapi.Interfaces.Redis
{
    public interface IRedisCache
    {
        Task<string> CacheKey(string key, Func<Task<KeyModel>> readKeyFunction);
        Task CacheData(string key, object value, TimeSpan expire);
        Task<string> GetCachedData(string key);
        Task DeleteCache(string key);
    }
}
