using StackExchange.Redis;

namespace data.Redis
{
    public interface IRedisDbContext
    {
        IDatabase GetDatabase();
    }
}
