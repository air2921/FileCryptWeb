using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Helpers;
using webapi.Interfaces.Services;

namespace webapi.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class BearerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BearerMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public BearerMiddleware(RequestDelegate next, ILogger<BearerMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context, FileCryptDbContext dbContext, ITokenService tokenService)
        {
            string jwt = null;

            context.Request.Cookies.TryGetValue(ImmutableData.JWT_COOKIE_KEY, out string? jwtCookie);
            var jwtHeaders = context.Request.Headers[ImmutableData.JWT_TOKEN_HEADER_NAME];

            if (!string.IsNullOrWhiteSpace(jwtCookie))
                jwt = jwtCookie;
            else
                jwt = jwtHeaders.ToString();

            if (!string.IsNullOrWhiteSpace(jwt))
            {
                context.Request.Headers.Add("Authorization", $"Bearer {jwt}");
                AddSecurityHeaders(context);

                await _next(context); return;
            }

            string refresh = null;
            var refreshHeaders = context.Request.Headers[ImmutableData.REFRESH_TOKEN_HEADER_NAME];
            context.Request.Cookies.TryGetValue(ImmutableData.REFRESH_COOKIE_KEY, out string? refreshCookie);

            if (!string.IsNullOrWhiteSpace(refreshCookie))
                refresh = refreshCookie;
            else
                refresh = refreshHeaders.ToString();

            if (!string.IsNullOrWhiteSpace(refresh))
            {
                if (_env.IsDevelopment())
                    _logger.LogWarning(tokenService.HashingToken(refresh));

                var userAndToken = await dbContext.Tokens
                    .Where(t => t.refresh_token == tokenService.HashingToken(refresh))
                    .Join(dbContext.Users, token => token.user_id, user => user.id, (token, user) => new { token, user })
                    .FirstOrDefaultAsync();

                if (userAndToken is null || userAndToken.token.expiry_date < DateTime.UtcNow || userAndToken.user.is_blocked == true)
                {
                    tokenService.DeleteTokens();
                    await _next(context); return;
                }

                string createdJWT = tokenService.GenerateJwtToken(userAndToken.user, ImmutableData.JwtExpiry);
                var jwtCookieOptions = tokenService.SetCookieOptions(ImmutableData.JwtExpiry);

                context.Response.Cookies.Append(ImmutableData.JWT_COOKIE_KEY, createdJWT, jwtCookieOptions);

                context.Request.Headers.Add("Authorization", $"Bearer {createdJWT}");
                AddSecurityHeaders(context);
            }

            await _next(context); return;
        }

        private void AddSecurityHeaders(HttpContext context)
        {
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Add("X-Xss-Protection", "1");
            context.Response.Headers.Add("X-Frame-Options", "DENY");
            context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self' https://cdnjs.cloudflare.com; style-src 'self' https://fonts.googleapis.com; font-src 'self' https://fonts.gstatic.com; img-src 'self' data:;");
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
