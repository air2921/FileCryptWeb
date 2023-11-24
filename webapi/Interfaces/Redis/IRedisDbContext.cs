using StackExchange.Redis;

namespace webapi.Interfaces.Redis
{
    public interface IRedisDbContext
    {
        IDatabase GetDatabase();
    }
}
