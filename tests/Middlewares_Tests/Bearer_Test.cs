using webapi;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using webapi.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using webapi.Models;
using webapi.Helpers;
using webapi.DB.Ef;
using webapi.Helpers.Abstractions;

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
                        Assert.True(context.Request.Headers.ContainsKey(ImmutableData.JWT_TOKEN_HEADER_NAME));
                        Assert.True(context.Request.Headers.ContainsKey("Authorization"));
                        await Task.CompletedTask;
                    });
                });

            using var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "/");
            request.Headers.Add(ImmutableData.JWT_TOKEN_HEADER_NAME, "hdfjkyhgdfuigy9d8gjkhdfhjgkdhfgkjldhlkgfjkd");
            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task CookieHasNotJwt_CookieHasRefresh_Header_SuccessAdded_JwtUpdated()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseUrls("http://localhost:2922")
                .ConfigureServices(services =>
                {
                    services.AddDbContext<FileCryptDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDatabase");
                    });

                    services.AddScoped<ITokenService, FakeTokenService>();
                })
                .Configure(async app =>
                {
                    using (var scope = app.ApplicationServices.CreateScope())
                    {
                        var services = scope.ServiceProvider;
                        var dbContext = services.GetRequiredService<FileCryptDbContext>();

                        var user = new UserModel
                        {
                            id = 1,
                            username = "air",
                            role = "Admin",
                            email = "air147@gmail.com",
                            is_2fa_enabled = true,
                            is_blocked = false,
                            password = "kjdfghjdfhgjkdfhgjkdfshglkjhgkjdfhklghjlkdshfklghkjd"
                        };

                        var token = new TokenModel
                        {
                            token_id = 1,
                            user_id = 1,
                            refresh_token = "hdfjkyhgdfuigy9d8gjkhdfhjgkdhfgkjldhlkgfjkd",
                            expiry_date = DateTime.UtcNow.AddDays(10)
                        };

                        dbContext.Users.Add(user);
                        dbContext.Tokens.Add(token);
                        await dbContext.SaveChangesAsync();
                    }

                    app.UseMiddleware<BearerMiddleware>();
                    app.Run(async context =>
                    {
                        Assert.True(context.Request.Headers.ContainsKey(ImmutableData.REFRESH_TOKEN_HEADER_NAME));
                        Assert.True(context.Request.Headers.ContainsKey("Authorization"));
                        Assert.True(context.Response.Headers["Set-Cookie"].ToString().Contains(ImmutableData.JWT_COOKIE_KEY));
                        Assert.True(context.Response.Headers["Set-Cookie"].ToString().Contains("FAKE_JSON.WEB.TOKEN"));
                        await Task.CompletedTask;
                    });
                });

            using var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "/");
            request.Headers.Add(ImmutableData.REFRESH_TOKEN_HEADER_NAME, "hdfjkyhgdfuigy9d8gjkhdfhjgkdhfgkjldhlkgfjkd");
            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task CookieHasNotJwt_CookieHasNotRefresh_NoneHeader_401StatusCode()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseUrls("http://localhost:2923")
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
                return "FAKE_JSON.WEB.TOKEN";
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
