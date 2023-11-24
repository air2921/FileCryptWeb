using StackExchange.Redis;
using webapi.Interfaces.Redis;

namespace webapi.DB.RedisDb
{
    public class RedisDbContext : IRedisDbContext
    {
        private readonly IDatabase _database;
        private readonly ConnectionMultiplexer _redis;
        private readonly IConfiguration _configuration;

        public RedisDbContext(IConfiguration configuration)
        {
            _configuration = configuration;

            string connectionString = _configuration.GetConnectionString("RedisConnection")!;

            _redis = ConnectionMultiplexer.Connect(connectionString);

            _database = _redis.GetDatabase();
        }

        public IDatabase GetDatabase()
        {
            return _database;
        }
    }
}
