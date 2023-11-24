using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.SQL.API;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL.API
{
    public class UpdateAPI : IUpdateAPI
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IRedisCache _redisCache;

        public UpdateAPI(FileCryptDbContext dbContext, IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _redisCache = redisCache;
        }

        public async Task UpdateApiSetting(ApiModel apiModel)
        {
            var api = await _dbContext.API.FirstOrDefaultAsync(a => a.user_id == apiModel.user_id) ??
                throw new ApiException(ExceptionApiMessages.UserApiNotFound);

            if (!string.IsNullOrWhiteSpace(apiModel.api_key))
                api.api_key = apiModel.api_key;

            if (apiModel.remote_ip is not null)
                api.remote_ip = apiModel.remote_ip;

            if (apiModel.is_tracking_ip is not null)
                api.is_tracking_ip = apiModel.is_tracking_ip;

            if (apiModel.is_allowed_requesting is not null)
                api.is_allowed_requesting = apiModel.is_allowed_requesting;

            if (apiModel.is_allowed_unknown_ip is not null)
                api.is_allowed_unknown_ip = apiModel.is_allowed_unknown_ip;

            await _redisCache.DeleteCache(api.api_key);

            await _dbContext.SaveChangesAsync();
        }
    }
}
