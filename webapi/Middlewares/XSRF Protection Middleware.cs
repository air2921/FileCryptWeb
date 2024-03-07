using Microsoft.AspNetCore.Antiforgery;
using webapi.Helpers;
using webapi.Interfaces.Services;

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
            var requestToken = _antiforgery.GetAndStoreTokens(context).RequestToken;
            context.Response.Cookies.Append(ImmutableData.XSRF_COOKIE_KEY, requestToken,
                new CookieOptions { HttpOnly = true, MaxAge = TimeSpan.FromMinutes(90), Secure = true, SameSite = SameSiteMode.None });

            context.Request.Headers.Append(ImmutableData.XSRF_HEADER_NAME, requestToken);

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
