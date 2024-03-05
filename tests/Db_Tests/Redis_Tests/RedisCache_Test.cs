using Newtonsoft.Json;
using StackExchange.Redis;
using webapi.DB.RedisDb;
using webapi.Interfaces.Redis;

namespace tests.Db_Tests.Redis_Tests
{
    public class RedisCache_Test
    {
        [Fact]
        public async Task CacheDataAndGetCachedData()
        {
            var connection = ConnectionMultiplexer.Connect("localhost");
            var redisCache = new RedisCache(new TestRedisDbContext(connection));

            var srcData = new TestObject { Name = "Air", Age = 20, UserId = 1 };

            await redisCache.CacheData("testKey", srcData, TimeSpan.FromMinutes(1));

            var cachedDataJson = await redisCache.GetCachedData("testKey");
            Assert.NotNull(cachedDataJson);

            var cachedData = JsonConvert.DeserializeObject<TestObject>(cachedDataJson);

            Assert.Equal(srcData.UserId, cachedData.UserId);
            Assert.Equal(srcData.Name, cachedData.Name);
            Assert.Equal(srcData.Age, cachedData.Age);
        }

        [Fact]
        public async Task DeleteCache()
        {
            var connection = ConnectionMultiplexer.Connect("localhost");
            var redisCache = new RedisCache(new TestRedisDbContext(connection));

            var srcData = new TestObject { Name = "Air", Age = 20, UserId = 1 };

            await redisCache.CacheData("testKey", srcData, TimeSpan.FromMinutes(1));
            await redisCache.DeleteCache("testKey");

            var cachedDataJson = await redisCache.GetCachedData("testKey");

            Assert.Null(cachedDataJson);
        }

        [Fact]
        public async Task DeleteCacheByPattern()
        {
            var connection = ConnectionMultiplexer.Connect("localhost");
            var redisCache = new RedisCache(new TestRedisDbContext(connection));

            var srcData = new TestObject { Name = "Air", Age = 20, UserId = 1 };

            await redisCache.CacheData("testKey", srcData, TimeSpan.FromMinutes(1));
            await redisCache.DeteteCacheByKeyPattern("Key");

            var cachedDataJson = await redisCache.GetCachedData("testKey");

            Assert.Null(cachedDataJson);
        }

        [Fact]
        public async Task DeleteRedisCache_DeletesCacheForAllUsers()
        {
            var connection = ConnectionMultiplexer.Connect("localhost");
            var redisCache = new RedisCache(new TestRedisDbContext(connection));

            var data = new List<TestObject>
            {
                new TestObject { Name = "Air", Age = 20, UserId = 1 },
                new TestObject { Name = "Air", Age = 20, UserId = 1 },
                new TestObject { Name = "Air", Age = 20, UserId = 1 },
                new TestObject { Name = "Zanfery", Age = 20, UserId = 6 },
                new TestObject { Name = "baby_mary", Age = 19, UserId = 12 },
                new TestObject { Name = "common", Age = 20, UserId = 87 },
                new TestObject { Name = "Zanfery", Age = 20, UserId = 6 },
            };

            Func<TestObject, int> getUserId = dataItem => dataItem.UserId;

            foreach (var item in data)
                await redisCache.CacheData($"testPrefix{item.UserId}", data, TimeSpan.FromMinutes(1));

            await redisCache.DeleteRedisCache(data, "testPrefix", getUserId);

            foreach (var item in data)
            {
                var cacheKey = $"testPrefix{item.UserId}";
                var cachedData = await redisCache.GetCachedData(cacheKey);

                Assert.Null(cachedData);
            }
        }
    }

    internal class TestRedisDbContext : IRedisDbContext
    {
        private readonly IDatabase _database;

        public TestRedisDbContext(ConnectionMultiplexer connection)
        {
            _database = connection.GetDatabase();
        }

        public IDatabase GetDatabase()
        {
            return _database;
        }
    }

    internal class TestObject
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
