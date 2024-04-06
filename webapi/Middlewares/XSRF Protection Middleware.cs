using Microsoft.AspNetCore.Antiforgery;
using webapi.Helpers;

namespace webapi.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class XSRFProtectionMiddleware(RequestDelegate next, IAntiforgery antiforgery)
    {
        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey(ImmutableData.XSRF_HEADER_NAME))
            {
                var xsrf = context.Request.Cookies[ImmutableData.XSRF_COOKIE_KEY];
                if (!string.IsNullOrWhiteSpace(xsrf))
                    context.Request.Headers.Append(ImmutableData.XSRF_HEADER_NAME, xsrf);
            }

            var requstToken = antiforgery.GetAndStoreTokens(context).RequestToken;
            if (requstToken is not null)
            {
                context.Response.Cookies.Append(
                ImmutableData.XSRF_COOKIE_KEY,
                requstToken,
                new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    MaxAge = TimeSpan.FromMinutes(60)
                });
            }

            await next(context);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class XSRF_Protection_MiddlewareExtensions
    {
        public static IApplicationBuilder UseXSRF(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<XSRFProtectionMiddleware>();
        }
    }
}
