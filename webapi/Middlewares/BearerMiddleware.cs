using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Interfaces.Services;
using webapi.Models;

namespace webapi.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class BearerMiddleware
    {
        private readonly RequestDelegate _next;

        public BearerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, FileCryptDbContext dbContext, ITokenService tokenService)
        {
            if (context.Request.Cookies.TryGetValue("JwtToken", out string? jwt))
            {
                context.Request.Headers.Add("Authorization", $"Bearer {jwt}");
                await _next(context);
                return;
            }
            else
            {
                if (context.Request.Cookies.TryGetValue("RefreshToken", out string? refresh))
                {
                    var userAndToken = await dbContext.Tokens
                        .Where(t => t.refresh_token == tokenService.HashingToken(refresh))
                        .Join(dbContext.Users, token => token.user_id, user => user.id, (token, user) => new { token, user })
                        .FirstOrDefaultAsync();

                    if (userAndToken is not null && userAndToken.token.expiry_date.HasValue)
                    {
                        if (userAndToken.token.expiry_date > DateTime.UtcNow)
                        {
                            var userModel = new UserModel
                            {
                                id = userAndToken.user.id,
                                username = userAndToken.user.username,
                                email = userAndToken.user.email,
                                role = userAndToken.user.role
                            };

                            string createdJWT = tokenService.GenerateJwtToken(userModel, 20);
                            var jwtCookieOptions = tokenService.SetCookieOptions(TimeSpan.FromMinutes(20));

                            context.Response.Cookies.Append("JwtToken", createdJWT, jwtCookieOptions);

                            context.Request.Headers.Add("Authorization", $"Bearer {createdJWT}");
                        }
                    }
                }
            }
            await _next(context);
            return;
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class BearerMiddlewareExtensions
    {
        public static IApplicationBuilder UseBearer(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BearerMiddleware>();
        }
    }
}
