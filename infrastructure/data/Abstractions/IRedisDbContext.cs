using StackExchange.Redis;

namespace data.Abstractions
{
    public interface IRedisDbContext
    {
        IDatabase GetDatabase();
    }
}
