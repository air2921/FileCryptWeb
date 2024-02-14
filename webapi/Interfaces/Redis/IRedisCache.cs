using webapi.Models;

namespace webapi.Interfaces.Redis
{
    public interface IRedisCache
    {
        Task<string> CacheKey(string key, int userId);
        Task CacheData(string key, object value, TimeSpan expire);
        Task<string> GetCachedData(string key);
        Task DeleteCache(string key);
        Task DeteteCacheByKeyPattern(string key);
    }
}
