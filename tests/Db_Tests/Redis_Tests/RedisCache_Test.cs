using Newtonsoft.Json;
using StackExchange.Redis;
using webapi.DB.RedisDb;
using webapi.Interfaces.Redis;

namespace tests.Db_Tests.Redis_Tests
{
    // !!!!!!!!!!!!!!!!!!!!!!!!!
    // Do tests latest
    // !!!!!!!!!!!!!!!!!!!!!!!!!
    // Now tests works incorrect
    // !!!!!!!!!!!!!!!!!!!!!!!!!

    //public class RedisCache_Test
    //{
    //    [Fact]
    //    public async Task CacheDataAndGetCachedData()
    //    {
    //        var mockContext = new Mock<IRedisDbContext>();
    //        var mockDatabase = new Mock<IDatabase>();

    //        mockContext.Setup(c => c.GetDatabase()).Returns(mockDatabase.Object);

    //        var redisCache = new RedisCache(mockContext.Object);

    //        var srcData = new TestObject { Name = "Air", Age = 20, UserId = 1 };

    //        mockDatabase.Setup(x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
    //                    .Returns(Task.FromResult(true));
    //        await redisCache.CacheData("testKey", srcData, TimeSpan.FromMinutes(1));
    //        mockDatabase.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
    //                    .ReturnsAsync(JsonConvert.SerializeObject(srcData));

    //        var cachedDataJson = await redisCache.GetCachedData("testKey");
    //        Assert.NotNull(cachedDataJson);

    //        var cachedData = JsonConvert.DeserializeObject<TestObject>(cachedDataJson);

    //        Assert.Equal(srcData.UserId, cachedData!.UserId);
    //        Assert.Equal(srcData.Name, cachedData.Name);
    //        Assert.Equal(srcData.Age, cachedData.Age);
    //    }

    //    [Fact]
    //    public async Task DeleteCache()
    //    {
    //        var mockContext = new Mock<IRedisDbContext>();
    //        var mockDatabase = new Mock<IDatabase>();

    //        mockContext.Setup(c => c.GetDatabase()).Returns(mockDatabase.Object);

    //        var redisCache = new RedisCache(mockContext.Object);

    //        var srcData = new TestObject { Name = "Air", Age = 20, UserId = 1 };

    //        mockDatabase.Setup(x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
    //                    .Returns(Task.FromResult(true));
    //        await redisCache.CacheData("testKey", srcData, TimeSpan.FromMinutes(1));
    //        await redisCache.DeleteCache("testKey");

    //        var cachedDataJson = await redisCache.GetCachedData("testKey");
    //        Assert.Null(cachedDataJson);
    //    }

    //    [Fact]
    //    public async Task DeleteCacheByPattern()
    //    {
    //        var mockContext = new Mock<IRedisDbContext>();
    //        var mockDatabase = new Mock<IDatabase>();

    //        mockContext.Setup(c => c.GetDatabase()).Returns(mockDatabase.Object);
    //        var redisCache = new RedisCache(mockContext.Object);

    //        var srcData = new TestObject { Name = "Air", Age = 20, UserId = 1 };

    //        mockDatabase.Setup(x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
    //                    .Returns(Task.FromResult(true));
    //        await redisCache.CacheData("testKey", srcData, TimeSpan.FromMinutes(1));
    //        await redisCache.DeteteCacheByKeyPattern("Key");

    //        var cachedDataJson = await redisCache.GetCachedData("testKey");
    //        Assert.Null(cachedDataJson);
    //    }
    //}

    //public class TestObject
    //{
    //    public int UserId { get; set; }
    //    public string Name { get; set; }
    //    public int Age { get; set; }
    //}
}
