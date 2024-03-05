using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using webapi.Middlewares;

namespace tests.Middlewares_Tests
{
    public class LogMiddleware_Test
    {
        [Fact]
        public async Task Invoke_AuthenticatedUser_LogsUserInfoAndRequestInfo()
        {
            var context = new DefaultHttpContext();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testUser"),
                new Claim(ClaimTypes.NameIdentifier, "userId123"),
                new Claim(ClaimTypes.Role, "Admin"),
            }, "mock"));

            context.User = user;

            var fakeLogger = new FakeLogger<LogMiddleware>();
            var middleware = new LogMiddleware((innerHttpContext) => Task.CompletedTask, fakeLogger);

            await middleware.Invoke(context);

            Assert.Contains(fakeLogger.LoggedMessages, msg =>
                msg.Contains("testUser") &&
                msg.Contains("userId123") &&
                msg.Contains("Admin"));

            Assert.Contains(fakeLogger.LoggedMessages, msg =>
                msg.Contains(context.Request.Path.ToString()) &&
                msg.Contains(context.Request.Method.ToString()));
        }

        [Fact]
        public async Task Invoke_UnauthenticatedUser_LogsOnlyRequestInfo()
        {
            var context = new DefaultHttpContext();
            var fakeLogger = new FakeLogger<LogMiddleware>();
            var middleware = new LogMiddleware((innerHttpContext) => Task.CompletedTask, fakeLogger);

            await middleware.Invoke(context);

            Assert.DoesNotContain(fakeLogger.LoggedMessages, msg =>
                msg.Contains("testUser") &&
                msg.Contains("userId123") &&
                msg.Contains("Admin"));

            Assert.Contains(fakeLogger.LoggedMessages, msg =>
                msg.Contains(context.Request.Path.ToString()) &&
                msg.Contains(context.Request.Method.ToString()));
        }
    }
}
