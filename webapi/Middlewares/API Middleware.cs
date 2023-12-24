using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Interfaces.Redis;
using webapi.Services;

namespace webapi.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ApiMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiMiddleware> _logger;

        public ApiMiddleware(RequestDelegate next, ILogger<ApiMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IRedisCache redisCache, FileCryptDbContext dbContext)
        {
            if (!context.Request.Path.Value.Contains("api/public", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            string? apiKey = context.Request.Headers[Constants.API_HEADER_NAME];
            if (apiKey is null)
            {
                await _next(context);
                return;
            }

            try
            {
                var isAllowRequest = await redisCache.GetCachedData(apiKey);
                bool isAllowed = bool.Parse(isAllowRequest);

                if (isAllowed == false)
                {
                    context.Response.StatusCode = 403;
                    return;
                }

                await _next(context);
                return;
            }
            catch (KeyNotFoundException)
            {
                try
                {
                    var isAllowed = await CheckAndCacheData(context, redisCache, dbContext, apiKey);
                    
                    if(isAllowed)
                    {
                        await _next(context);
                        return;
                    }
                    else
                    {
                        context.Response.StatusCode = 403;
                        return;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    context.Response.StatusCode = int.Parse(ex.Message);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString(), nameof(ApiMiddleware));
                context.Response.StatusCode = 500;
                return;
            }
        }

        private async Task<bool> CheckAndCacheData(
            HttpContext context,
            IRedisCache redisCache,
            FileCryptDbContext dbContext,
            string apiKey)
        {

            var api = await dbContext.API.FirstOrDefaultAsync(a => a.api_key == apiKey);

            if (api is null)
            {
                context.Response.StatusCode = 401;
                throw new InvalidOperationException("401");
            }
            if (api.is_allowed_requesting == false)
            {
                await redisCache.CacheData(apiKey, false.ToString(), TimeSpan.FromDays(1));
                context.Response.StatusCode = 403;
                throw new InvalidOperationException("403");
            }

            await redisCache.CacheData(apiKey, true.ToString(), TimeSpan.FromDays(1));
            return true;
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ApiMiddlewareExtensions
    {
        public static IApplicationBuilder UseAPI(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiMiddleware>();
        }
    }
}
