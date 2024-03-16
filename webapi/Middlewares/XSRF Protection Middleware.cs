using Microsoft.AspNetCore.Antiforgery;
using webapi.Helpers;

namespace webapi.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class XSRFProtectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAntiforgery _antiforgery;

        public XSRFProtectionMiddleware(RequestDelegate next, IAntiforgery antiforgery)
        {
            _next = next;
            _antiforgery = antiforgery;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.Cookies.Append(
            ImmutableData.XSRF_COOKIE_KEY,
            _antiforgery.GetAndStoreTokens(context).RequestToken,
            new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                MaxAge = TimeSpan.FromMinutes(60)
            });

            var xsrf = context.Request.Cookies[ImmutableData.XSRF_COOKIE_KEY];
            if (!string.IsNullOrWhiteSpace(xsrf))
                context.Request.Headers.Append(ImmutableData.XSRF_HEADER_NAME, xsrf);

            await _next(context);
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
