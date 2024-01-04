using Org.BouncyCastle.Asn1.Ocsp;
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
            var userContext = context.User;

            string? claimsUsername = null;
            string? claimId = null;
            string? claimRole = null;

            var path = context.Request.Path.ToString();
            var method = context.Request.Method.ToString();

            if (userContext.Identity.IsAuthenticated)
            {
                if (userContext.HasClaim(u => u.Type == ClaimTypes.Name))
                {
                    claimsUsername = userContext.FindFirstValue(ClaimTypes.Name);
                }
                if (userContext.HasClaim(u => u.Type == ClaimTypes.NameIdentifier))
                {
                    claimId = userContext.FindFirstValue(ClaimTypes.NameIdentifier);
                }
                if (userContext.HasClaim(u => u.Type == ClaimTypes.Role))
                {
                    claimRole = userContext.FindFirstValue(ClaimTypes.Role);
                }
            }

            var user = new
            {
                username = claimsUsername,
                id = claimId,
                role = claimRole,
            };

            var request = new
            {
                path,
                method,
            };

            var requestData = $"{user} {request}";

            _logger.LogInformation(requestData);

            await _next(context);

            var statusCode = context.Response.StatusCode;

            _logger.LogInformation($"Status Code: {statusCode}");
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
