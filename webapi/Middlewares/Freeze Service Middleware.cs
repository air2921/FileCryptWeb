using Newtonsoft.Json;
using webapi.Interfaces.Redis;
using webapi.Services;

namespace webapi.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class FreezeServiceMiddleware
    {
        private readonly RequestDelegate _next;

        public FreezeServiceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IRedisCache redisCache)
        {
            try
            {
                if (!IsAdminEndpoint(context.Request.Path))
                {
                    var freezed = await IsServiceFreeze(redisCache);
                    if (freezed)
                    {
                        context.Response.StatusCode = 503;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new { message = "The service was frozen for technical work. We'll finish as quickly as we can" });
                        return;
                    }
                    await _next(context);
                    return;
                }
                await _next(context);
                return;
            }
            catch (InvalidOperationException)
            {
                await _next(context);
                return;
            }
            catch (FormatException)
            {
                await _next(context);
                return;
            }
        }

        private static bool IsAdminEndpoint(PathString endpointPath)
        {
            return endpointPath.Value!.Contains("admin", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<bool> IsServiceFreeze(IRedisCache redisCache)
        {
            try
            {
                var stringFlag = await redisCache.GetCachedData(Constants.SERVICE_FREEZE_FLAG) ??
                    throw new InvalidOperationException();

                return JsonConvert.DeserializeObject<bool>(stringFlag);
            }
            catch (FormatException)
            {
                throw;
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class FreezeServiceMiddlewareExtensions
    {
        public static IApplicationBuilder UseFreeze(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FreezeServiceMiddleware>();
        }
    }
}
