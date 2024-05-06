using application.Abstractions.Endpoints.Account;
using application.Helpers;

namespace webapi.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class BearerMiddleware(RequestDelegate next, ILogger<BearerMiddleware> logger, IWebHostEnvironment env)
    {
        public async Task Invoke(HttpContext context, ISessionService service)
        {
            if (context.Request.Headers.ContainsKey(ImmutableData.NONE_BEARER))
            {
                await next(context);
                return;
            }

            context.Request.Cookies.TryGetValue(ImmutableData.JWT_COOKIE_KEY, out string? requestJwt);
            if (!string.IsNullOrWhiteSpace(requestJwt))
            {
                context.Request.Headers.Append("Authorization", $"Bearer {requestJwt}");
                AddSecurityHeaders(context);

                await next(context);
                return;
            }

            context.Request.Cookies.TryGetValue(ImmutableData.REFRESH_COOKIE_KEY, out string? requestRefresh);
            if (!string.IsNullOrWhiteSpace(requestRefresh))
            {
                if (env.IsDevelopment())
                    logger.LogWarning(requestRefresh);

                var response = await service.UpdateJwt(requestRefresh);
                if (!response.IsSuccess)
                {
                    context.Response.Cookies.Delete(ImmutableData.REFRESH_COOKIE_KEY);
                    await next(context);
                    return;
                }

                if (response.ObjectData is not string newJwt)
                {
                    await next(context);
                    return;
                }

                context.Response.Cookies.Append(ImmutableData.JWT_COOKIE_KEY, newJwt, new CookieOptions
                {
                    MaxAge = ImmutableData.JwtExpiry,
                    Secure = true,
                    HttpOnly = false,
                    SameSite = SameSiteMode.None,
                    IsEssential = false
                });
                context.Request.Headers.Append("Authorization", $"Bearer {newJwt}");
                AddSecurityHeaders(context);
            }

            await next(context);
            return;
        }

        private void AddSecurityHeaders(HttpContext context)
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Xss-Protection", "1");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self' https://cdnjs.cloudflare.com; style-src 'self' https://fonts.googleapis.com; font-src 'self' https://fonts.gstatic.com; img-src 'self' data:;");
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
