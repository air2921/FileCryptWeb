using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using webapi.DB;
using webapi.Interfaces.Redis;

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

            string? apiKey = context.Request.Headers["x-API_Key"];
            var remoteIP = context.Connection.RemoteIpAddress;
            if (apiKey is null)
            {
                await _next(context);
                return;
            }

            try
            {
                var apiUserCacheData = await redisCache.GetCachedData(apiKey);

                var JsonApidata = JObject.Parse(apiUserCacheData);

                var ip = JsonApidata["ip"].ToString();
                var isAllowRequest = bool.Parse(JsonApidata["isAllowRequest"].ToString());
                var isAllowUnknownIp = bool.Parse(JsonApidata["isAllowUnknowIP"].ToString());
                var isTracking = bool.Parse(JsonApidata["isTracking"].ToString());

                if (isAllowRequest == false)
                {
                    context.Response.StatusCode = 403;
                    return;
                }
                if (isAllowUnknownIp == false && !ip.Equals(remoteIP.ToString()))
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
                    await CheckAndCacheData(context, redisCache, dbContext, apiKey, remoteIP);

                    await _next(context);
                }
                catch (InvalidOperationException ex)
                {
                    context.Response.StatusCode = int.Parse(ex.Message);
                    return;
                }
                finally
                {

                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString(), nameof(ApiMiddleware));
            }
        }

        private async Task CheckAndCacheData(
            HttpContext context,
            IRedisCache redisCache,
            FileCryptDbContext dbContext,
            string apiKey,
            IPAddress? remoteIP)
        {

            var api = await dbContext.API.FirstOrDefaultAsync(a => a.api_key == apiKey);

            if (api is null)
            {
                context.Response.StatusCode = 401;
                throw new InvalidOperationException("401");
            }
            if (api.is_allowed_requesting == false)
            {
                context.Response.StatusCode = 403;
                throw new InvalidOperationException("403");
            }
            if (api.is_allowed_unknown_ip == false && !api.remote_ip.Equals(remoteIP))
            {
                context.Response.StatusCode = 403;
                throw new InvalidOperationException("403");
            }
            if (api.remote_ip is null && api.is_tracking_ip == true)
            {
                api.remote_ip = remoteIP;
                await dbContext.SaveChangesAsync();
            }

            var apiUserCacheData = new
            {
                ip = api.remote_ip.ToString(),
                isAllowRequest = api.is_allowed_requesting,
                isAllowUnknowIP = api.is_allowed_unknown_ip,
                isTracking = api.is_tracking_ip
            };

            var serializedAPIData = JsonConvert.SerializeObject(apiUserCacheData);

            await redisCache.CacheData(apiKey, serializedAPIData, TimeSpan.FromDays(1));
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
