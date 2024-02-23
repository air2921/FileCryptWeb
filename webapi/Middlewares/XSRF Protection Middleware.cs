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

        public async Task Invoke(HttpContext context, ITokenService tokenService)
        {
            bool antiforgeryCookieExists = context.Request.Cookies.Any(cookie => cookie.Key.StartsWith(".AspNetCore.Antiforgery."));
            context.Request.Cookies.TryGetValue(ImmutableData.XSRF_COOKIE_KEY, out string? xsrf);

            if (string.IsNullOrEmpty(xsrf) || !antiforgeryCookieExists)
            {
                var requestToken = _antiforgery.GetAndStoreTokens(context).RequestToken;
                context.Response.Cookies.Append(ImmutableData.XSRF_COOKIE_KEY, requestToken,
                    tokenService.SetCookieOptions(TimeSpan.FromMinutes(90)));
            }
            else
            {
                context.Request.Headers.Append(ImmutableData.XSRF_HEADER_NAME, xsrf);
            }

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
