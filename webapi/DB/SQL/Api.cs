using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL
{
    public class Api : ICreate<ApiModel>, IRead<ApiModel>, IUpdate<ApiModel>, IDelete<ApiModel>, IDeleteByName<ApiModel>
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IRedisCache _redisCache;

        public Api(FileCryptDbContext dbContext, IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _redisCache = redisCache;
        }

        public async Task Create(ApiModel apiModel)
        {
            var newApiModel = new ApiModel
            {
                user_id = apiModel.user_id,
                api_key = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString(),
                remote_ip = apiModel.remote_ip,
                is_tracking_ip = true,
                is_allowed_requesting = true,
                is_allowed_unknown_ip = false,
            };

            await _dbContext.AddAsync(newApiModel);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<ApiModel> ReadById(int id, bool? byForeign)
        {
            var api = _dbContext.API.AsQueryable();

            if (byForeign == false)
            {
                return await api.FirstOrDefaultAsync(a => a.api_id == id) ??
                    throw new ApiException(ExceptionApiMessages.ApiNotFound);
            }

            return await api.FirstOrDefaultAsync(a => a.user_id == id) ??
                throw new ApiException(ExceptionApiMessages.ApiNotFound);
        }

        public async Task<IEnumerable<ApiModel>> ReadAll()
        {
            return await _dbContext.API.ToListAsync() ??
                throw new ApiException(ExceptionApiMessages.NoOneApiNotFound);
        }

        public async Task Update(ApiModel apiModel, bool? byForeign)
        {
            if(byForeign == true)
            {
                var api = await _dbContext.API.FirstOrDefaultAsync(a => a.user_id == apiModel.user_id) ??
                    throw new ApiException(ExceptionApiMessages.ApiNotFound);

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
            }
            else
            {
                var api = await _dbContext.API.FirstOrDefaultAsync(a => a.api_id == apiModel.api_id) ??
                    throw new ApiException(ExceptionApiMessages.ApiNotFound);

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
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteById(int id)
        {
            var api = await _dbContext.API.FirstOrDefaultAsync(a => a.user_id == id) ??
                throw new ApiException(ExceptionApiMessages.ApiNotFound);

            await _redisCache.DeleteCache(api.api_key);

            _dbContext.Remove(api);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteByName(string apiKey)
        {
            var api = await _dbContext.API.FirstOrDefaultAsync(a => a.api_key == apiKey) ??
                throw new ApiException(ExceptionApiMessages.ApiNotFound);

            await _redisCache.DeleteCache(api.api_key);

            _dbContext.Remove(api);
            await _dbContext.SaveChangesAsync();
        }
    }
}
