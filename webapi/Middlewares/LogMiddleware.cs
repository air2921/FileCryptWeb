using System.Diagnostics;
using System.Security.Claims;
using webapi.Helpers;

namespace webapi.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class LogMiddleware(RequestDelegate next, ILogger<LogMiddleware> logger)
    {
        public async Task Invoke(HttpContext context, IRequest requestInfo)
        {
            var stopwatch = Stopwatch.StartNew();

            var userContext = context.User;
            var path = context.Request.Path.ToString();
            var method = context.Request.Method.ToString();
            var requestId = Guid.NewGuid().ToString();

            string claimsUsername = "Unknown";
            string claimId = "Unknown";
            string claimRole = "Unknown";

            if (userContext.Identity?.IsAuthenticated ?? false)
            {
                claimsUsername = userContext.FindFirstValue(ClaimTypes.Name) ?? "Unknown";
                claimId = userContext.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
                claimRole = userContext.FindFirstValue(ClaimTypes.Role) ?? "Unknown";
            }

            var user = new User { Username = claimsUsername, Id = claimId, Role = claimRole };
            var request = new RequestInfo { Path = path, Method = method };
            requestInfo.Token = requestId;

            logger.LogInformation($"Request Id: {requestId}\n" +
                                  $"Request entered at: {DateTime.UtcNow}\n" +
                                  $"User: {user.Username}, Id: {user.Id}, Role: {user.Role}\n" +
                                  $"Request: {request.Path}, Method: {request.Method}");

            context.Request.Headers.Append("X-REQUEST-TOKEN", requestId);

            await next(context);

            stopwatch.Stop();
            logger.LogInformation($"Request Id: {requestId}\n" +
                                  $"Request finished at: {DateTime.UtcNow}\n" +
                                  $"Total time request works: {stopwatch.Elapsed}\n" +
                                  $"Status Code: {context.Response.StatusCode}");
        }

        private class User
        {
            public string Username { get; set; } = "Unknown";
            public string Id { get; set; } = "Unknown";
            public string Role { get; set; } = "Unknown";
        }

        private class RequestInfo
        {
            public string Path { get; set; } = null!;
            public string Method { get; set; } = null!;
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
