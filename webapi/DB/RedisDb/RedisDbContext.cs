﻿using StackExchange.Redis;
using webapi.DB.Abstractions;
using webapi.Helpers;

namespace webapi.DB.RedisDb
{
    public class RedisDbContext(IConfiguration configuration) : IRedisDbContext
    {
        private readonly IDatabase _database = ConnectionMultiplexer.Connect(configuration.GetConnectionString(App.REDIS_DB)!).GetDatabase();
        public IDatabase GetDatabase() => _database;
    }
}
