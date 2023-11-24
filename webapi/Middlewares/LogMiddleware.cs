using System.Security.Claims;

namespace webapi.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class LogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogMiddleware> _logger;

        public LogMiddleware(RequestDelegate next, ILogger<LogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var user = context.User;

            string? claimsUsername = null;
            string? claimId = null;
            string? claimRole = null;

            var path = context.Request.Path.ToString();
            var method = context.Request.Method.ToString();
            var headers = context.Request.Headers.ToString();
            var remoteIP = context.Connection.RemoteIpAddress.ToString();

            if (user.Identity.IsAuthenticated)
            {
                if (user.HasClaim(u => u.Type == ClaimTypes.Name))
                {
                    claimsUsername = user.FindFirstValue(ClaimTypes.Name);
                }
                if (user.HasClaim(u => u.Type == ClaimTypes.NameIdentifier))
                {
                    claimId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                }
                if (user.HasClaim(u => u.Type == ClaimTypes.Role))
                {
                    claimRole = user.FindFirstValue(ClaimTypes.Role);
                }
            }

            var requestData = new
            {
                user = new
                {
                    username = claimsUsername,
                    id = claimId,
                    role = claimRole,
                },
                request = new
                {
                    path,
                    ip = remoteIP,
                    method,
                    headers
                }
            };

            _logger.LogWarning(requestData.ToString());

            await _next(context);

            var statusCode = context.Response.StatusCode;

            _logger.LogWarning($"Status Code: {statusCode}");
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class LogMiddlewareExtensions
    {
        public static IApplicationBuilder UseLog(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogMiddleware>();
        }
    }
}
