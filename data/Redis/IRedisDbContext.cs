using StackExchange.Redis;

namespace data_access.Redis
{
    public interface IRedisDbContext
    {
        IDatabase GetDatabase();
    }
}
