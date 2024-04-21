using application.Helpers;
using System.Security.Claims;
using webapi.Helpers;

namespace webapi.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class UserSessionMiddleware(RequestDelegate next)
    {
        public Task Invoke(HttpContext httpContext)
        {
            var userContext = httpContext.User;

            string? username = httpContext.Request.Cookies[ImmutableData.USERNAME_COOKIE_KEY];
            string? userId = httpContext.Request.Cookies[ImmutableData.USER_ID_COOKIE_KEY];
            string? userRole = httpContext.Request.Cookies[ImmutableData.ROLE_COOKIE_KEY];
            string? userAuth = httpContext.Request.Cookies[ImmutableData.IS_AUTHORIZED];

            var cookieOptions = new CookieOptions
            {
                MaxAge = ImmutableData.JwtExpiry,
                Secure = true,
                HttpOnly = false,
                SameSite = SameSiteMode.None,
                IsEssential = false
            };

            if (username is not null && userId is not null && userRole is not null && userAuth is not null)
                return next(httpContext);

            if (!userContext.Identity.IsAuthenticated)
            {
                httpContext.Response.Cookies.Append(ImmutableData.IS_AUTHORIZED, false.ToString(), cookieOptions);
                return next(httpContext);
            }

            httpContext.Response.Cookies.Append(ImmutableData.IS_AUTHORIZED, true.ToString(), cookieOptions);

            if (userContext.HasClaim(u => u.Type == ClaimTypes.Name))
            {
                var claimUsername = userContext.FindFirstValue(ClaimTypes.Name);
                httpContext.Response.Cookies.Append(ImmutableData.USERNAME_COOKIE_KEY, claimUsername!, cookieOptions);
            }
            if (userContext.HasClaim(u => u.Type == ClaimTypes.NameIdentifier))
            {
                var claimId = userContext.FindFirstValue(ClaimTypes.NameIdentifier);
                httpContext.Response.Cookies.Append(ImmutableData.USER_ID_COOKIE_KEY, claimId!, cookieOptions);
            }
            if (userContext.HasClaim(u => u.Type == ClaimTypes.Role))
            {
                var claimRole = userContext.FindFirstValue(ClaimTypes.Role);
                httpContext.Response.Cookies.Append(ImmutableData.ROLE_COOKIE_KEY, claimRole!, cookieOptions);
            }

            return next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class UserSessionMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserSession(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserSessionMiddleware>();
        }
    }
}
