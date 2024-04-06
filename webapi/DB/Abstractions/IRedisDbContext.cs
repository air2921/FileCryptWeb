using StackExchange.Redis;

namespace webapi.DB.Abstractions
{
    public interface IRedisDbContext
    {
        IDatabase GetDatabase();
    }
}
