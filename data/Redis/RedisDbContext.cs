using StackExchange.Redis;

namespace data.Redis
{
    public class RedisDbContext : IRedisDbContext
    {
        public string ConnectionString { get; set; }

        private readonly IDatabase _database;

        public RedisDbContext()
        {
            _database = ConnectionMultiplexer.Connect(ConnectionString!).GetDatabase();
        }

        public IDatabase GetDatabase() => _database;
    }
}
