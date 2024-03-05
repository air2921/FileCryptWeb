using webapi;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using webapi.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using webapi.Middlewares;
using webapi.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using webapi.Models;
using webapi.Helpers;
using System.Net;

namespace tests.Middlewares_Tests
{
    public class Bearer_Test : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public Bearer_Test(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CookieHasJwt_Header_SuccessAdded()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseUrls("http://localhost:2921")
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    services.AddDbContext<FileCryptDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDatabase");
                    });

                    services.AddScoped<ITokenService, FakeTokenService>();
                })
                .Configure(app =>
                {
                    app.UseMiddleware<BearerMiddleware>();
                    app.Run(async context =>
                    {
                        Assert.True(context.Request.Cookies.ContainsKey(ImmutableData.JWT_COOKIE_KEY));
                        Assert.True(context.Response.Headers.ContainsKey("Authorization"));
                        await Task.CompletedTask;
                    });
                });

            using var server = new TestServer(builder);
            using var handler = new HttpClientHandler();
            handler.CookieContainer.Add(new Uri("http://localhost:2921"), new Cookie(ImmutableData.JWT_COOKIE_KEY, "hdfjkyhgdfuigy9d8gjkhdfhjgkdhfgkjldhlkgfjkd"));
            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:2921") };

            var response = await client.GetAsync("/");

            response.EnsureSuccessStatusCode();
            response.Dispose();
        }

        [Fact]
        public async Task CookieHasNotJwt_CookieHasRefresh_Header_SuccessAdded_JwtUpdated()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseUrls("http://localhost:2921")
                .ConfigureServices(services =>
                {
                    services.AddDbContext<FileCryptDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDatabase");
                    });

                    services.AddScoped<ITokenService, FakeTokenService>();
                })
                .Configure(app =>
                {
                    app.UseMiddleware<BearerMiddleware>();
                    app.Run(async context =>
                    {
                        Assert.True(context.Request.Cookies.ContainsKey(ImmutableData.REFRESH_COOKIE_KEY));
                        Assert.True(context.Response.Headers.ContainsKey("Authorization"));
                        Assert.True(context.Response.Headers["Set-Cookie"].ToString().Contains(ImmutableData.JWT_COOKIE_KEY));
                        Assert.True(context.Response.Headers["Set-Cookie"].ToString().Contains("FAKE_JSON.WEB.TOKEN"));
                        await Task.CompletedTask;
                    });
                });

            using var server = new TestServer(builder);

            using var handler = new HttpClientHandler();
            handler.CookieContainer.Add(new Uri("http://localhost:2921"), new Cookie(ImmutableData.REFRESH_COOKIE_KEY, "hdfjkyhgdfuigy9d8gjkhdfhjgkdhfgkjldhlkgfjkd"));
            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:2921") };

            var response = await client.GetAsync("/");

            response.EnsureSuccessStatusCode();
            response.Dispose();
        }

        [Fact]
        public async Task CookieHasNotJwt_CookieHasNotRefresh_NoneHeader_401StatusCode()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseUrls("http://localhost:2921")
                .ConfigureServices(services =>
                {
                    services.AddDbContext<FileCryptDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDatabase");
                    });

                    services.AddScoped<ITokenService, FakeTokenService>();
                    services.AddRouting();
                    services.AddAuthentication();
                    services.AddAuthorization();
                })
                .Configure(app =>
                {
                    app.UseMiddleware<BearerMiddleware>();

                    app.Run(async context =>
                    {
                        Assert.False(context.Request.Cookies.ContainsKey(ImmutableData.REFRESH_COOKIE_KEY));
                        Assert.False(context.Request.Cookies.ContainsKey(ImmutableData.JWT_COOKIE_KEY));
                        Assert.False(context.Response.Headers.ContainsKey("Authorization"));
                        Assert.False(context.Response.Headers["Set-Cookie"].ToString().Contains(ImmutableData.JWT_COOKIE_KEY));
                        Assert.False(context.Response.Headers["Set-Cookie"].ToString().Contains("FAKE_JSON.WEB.TOKEN"));
                        await Task.CompletedTask;
                    });
                });

            using var server = new TestServer(builder);
            using var client = server.CreateClient();

            var response = await client.GetAsync("/");

            Assert.True(response.IsSuccessStatusCode);

            response.Dispose();
        }

        private class FakeTokenService : ITokenService
        {
            public string HashingToken(string token)
            {
                return token;
            }

            public void DeleteTokens()
            {

            }

            public string GenerateJwtToken(object user, TimeSpan expiry)
            {
                return "FAKE_JSON.WEB.TOKEN";
            }

            public CookieOptions SetCookieOptions(TimeSpan expiry)
            {
                return new CookieOptions
                {
                    Expires = DateTime.UtcNow.Add(expiry),
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
                    Secure = true
                };
            }

            public string GenerateRefreshToken()
            {
                throw new NotImplementedException();
            }

            public string GenerateJwtToken(UserModel userModel, TimeSpan expiry)
            {
                throw new NotImplementedException();
            }

            public Task UpdateJwtToken()
            {
                throw new NotImplementedException();
            }

            public void DeleteUserDataSession()
            {
                throw new NotImplementedException();
            }
        }
    }
}
