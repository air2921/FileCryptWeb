using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using webapi.Helpers;
using webapi.Middlewares;

namespace tests.Middlewares
{
    public class UserSession_Test
    {
        [Fact]
        public async Task Invoke_UnauthenticatedUser()
        {
            var context = new DefaultHttpContext();
            var middleware = new UserSessionMiddleware((innerHttpContext) => Task.CompletedTask);

            await middleware.Invoke(context);

            var setCookieHeader = context.Response.Headers[HeaderNames.SetCookie];
            Assert.NotNull(setCookieHeader);
            Assert.Contains($"{ImmutableData.IS_AUTHORIZED}={false}", setCookieHeader.ToString());
        }

        [Fact]
        public async Task Invoke_AuthenticatedUser()
        {
            var username = "testUsername";
            var id = "1";
            var role = "Admin";

            var context = new DefaultHttpContext();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Role, role),
            }, "mock"));

            context.User = user;
            var middleware = new UserSessionMiddleware((innerHttpContext) => Task.CompletedTask);

            await middleware.Invoke(context);

            var setCookieHeader = context.Response.Headers[HeaderNames.SetCookie];
            Assert.NotNull(setCookieHeader);
            Assert.Contains($"{ImmutableData.IS_AUTHORIZED}={true}", setCookieHeader.ToString());
            Assert.Contains($"{ImmutableData.USERNAME_COOKIE_KEY}={username}", setCookieHeader.ToString());
            Assert.Contains($"{ImmutableData.USER_ID_COOKIE_KEY}={id}", setCookieHeader.ToString());
            Assert.Contains($"{ImmutableData.ROLE_COOKIE_KEY}={role}", setCookieHeader.ToString());
        }
    }
}
