using StackExchange.Redis;

namespace data_access.Redis
{
    public class RedisDbContext : IRedisDbContext
    {
        public string ConnectionString { get; set; }

        private readonly IDatabase _database;

        public RedisDbContext()
        {
            if (ConnectionString is null)
                throw new ArgumentNullException(nameof(ConnectionString), "Redis connection string is null.");

            _database = ConnectionMultiplexer.Connect(ConnectionString).GetDatabase();
        }

        public IDatabase GetDatabase() => _database;
    }
}
