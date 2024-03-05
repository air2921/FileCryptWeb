using Microsoft.AspNetCore.Http;
using webapi.Interfaces.Redis;
using webapi.Middlewares;

namespace tests.Middlewares_Tests
{
    public class Freeze_Service_Test
    {
        [Fact]
        public async Task NonAdminEndpoint_NoFreeze_ReturnsNextMiddleware()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/endpoint";
            var redisCacheMock = new Mock<IRedisCache>();
            redisCacheMock.Setup(rc => rc.GetCachedData(It.IsAny<string>())).ReturnsAsync("false");
            var middleware = new FreezeServiceMiddleware((innerHttpContext) => Task.CompletedTask);

            await middleware.Invoke(context, redisCacheMock.Object);

            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact]
        public async Task NonAdminEndpoint_Freeze_Returns503StatusCode()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/endpoint";
            var redisCacheMock = new Mock<IRedisCache>();
            redisCacheMock.Setup(rc => rc.GetCachedData(It.IsAny<string>())).ReturnsAsync("true");
            var middleware = new FreezeServiceMiddleware((innerHttpContext) => Task.CompletedTask);

            var responseStream = new MemoryStream();
            context.Response.Body = responseStream;

            await middleware.Invoke(context, redisCacheMock.Object);

            responseStream.Position = 0;
            var reader = new StreamReader(responseStream);
            var responseBody = await reader.ReadToEndAsync();

            Assert.Equal(503, context.Response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
            Assert.Equal("{\"message\":\"The service was frozen for technical work. We'll finish as quickly as we can\"}", responseBody);
        }

        [Fact]
        public async Task AdminEndpoint_Freeze_ReturnsNextMiddleware()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/admin/endpoint";
            var redisCacheMock = new Mock<IRedisCache>();
            redisCacheMock.Setup(rc => rc.GetCachedData(It.IsAny<string>())).ReturnsAsync("true");
            var middleware = new FreezeServiceMiddleware((innerHttpContext) => Task.CompletedTask);

            await middleware.Invoke(context, redisCacheMock.Object);

            Assert.Equal(200, context.Response.StatusCode);
        }
    }
}
