using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace data_access.Redis
{
    public class RedisDbContext : IRedisDbContext
    {
        private readonly IDatabase _database;
        private readonly IConfiguration _config;

        public RedisDbContext(IConfiguration config)
        {
            _config = config;
            var connectionStr = _config.GetConnectionString("Redis") ?? throw new InvalidOperationException();
            var connection = ConnectionMultiplexer.Connect(connectionStr);
            _database = connection.GetDatabase();
        }

        public IDatabase GetDatabase() => _database;
    }
}
