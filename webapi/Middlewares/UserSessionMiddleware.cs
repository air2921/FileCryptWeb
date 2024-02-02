using System.Security.Claims;
using webapi.Services;

namespace webapi.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class UserSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public UserSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            var userContext = httpContext.User;

            string? username = httpContext.Request.Cookies[Constants.USERNAME_COOKIE_KEY];
            string? userId = httpContext.Request.Cookies[Constants.USER_ID_COOKIE_KEY];
            string? userRole = httpContext.Request.Cookies[Constants.ROLE_COOKIE_KEY];
            string? userAuth = httpContext.Request.Cookies[Constants.IS_AUTHORIZED];

            var cookieOptions = new CookieOptions
            {
                MaxAge = Constants.JwtExpiry,
                Secure = true,
                HttpOnly = false,
                SameSite = SameSiteMode.None,
                IsEssential = false
            };

            if (username is null || userId is null || userRole is null || userAuth is null)
            {
                if (userContext.Identity.IsAuthenticated)
                {
                    httpContext.Response.Cookies.Append(Constants.IS_AUTHORIZED, true.ToString(), cookieOptions);

                    if (userContext.HasClaim(u => u.Type == ClaimTypes.Name))
                    {
                        var claimUsername = userContext.FindFirstValue(ClaimTypes.Name);
                        httpContext.Response.Cookies.Append(Constants.USERNAME_COOKIE_KEY, claimUsername!, cookieOptions);
                    }
                    if (userContext.HasClaim(u => u.Type == ClaimTypes.NameIdentifier))
                    {
                        var claimId = userContext.FindFirstValue(ClaimTypes.NameIdentifier);
                        httpContext.Response.Cookies.Append(Constants.USER_ID_COOKIE_KEY, claimId!, cookieOptions);
                    }
                    if (userContext.HasClaim(u => u.Type == ClaimTypes.Role))
                    {
                        var claimRole = userContext.FindFirstValue(ClaimTypes.Role);
                        httpContext.Response.Cookies.Append(Constants.ROLE_COOKIE_KEY, claimRole!, cookieOptions);
                    }
                }
                else
                {
                    httpContext.Response.Cookies.Append(Constants.IS_AUTHORIZED, false.ToString(), cookieOptions);
                }
            }

            return _next(httpContext);
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
