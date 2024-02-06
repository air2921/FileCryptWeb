using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.SQL;
using webapi.Localization.Exceptions;
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
            try
            {
                var api = await _dbContext.API.Where(a => a.user_id == apiModel.user_id)
                    .ToListAsync();

                if (api.Count >= 5)
                    throw new ApiException("No more than 5 API Key for one user");

                await _dbContext.AddAsync(apiModel);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new ApiException(ExceptionApiMessages.AlreadyExists);
            }
        }

        public async Task<ApiModel> ReadById(int id, bool? byForeign)
        {
            return await _dbContext.API.FirstOrDefaultAsync(a => a.api_id == id) ??
                throw new ApiException(ExceptionApiMessages.ApiNotFound);
        }

        public async Task<IEnumerable<ApiModel>> ReadAll(int? user_id, int skip, int count)
        {
            var api = _dbContext.API.AsQueryable();

            if (user_id.HasValue)
                return await api.Where(a => a.user_id == user_id)
                    .Skip(skip)
                    .Take(count)
                    .ToListAsync();

            return await api
                .Skip(skip)
                .Take(count)
                .ToListAsync();
        }

        public async Task Update(ApiModel apiModel, bool? byForeign)
        {
            var api = await _dbContext.API.FirstOrDefaultAsync(a => a.api_id == apiModel.api_id);

            if (api is null)
                throw new ApiException(ExceptionApiMessages.ApiNotFound);

            api.api_key = apiModel.api_key;
            api.type = apiModel.type;
            api.expiry_date = apiModel.expiry_date;
            api.max_request_of_day = apiModel.max_request_of_day;
            api.is_blocked = apiModel.is_blocked;

            await _redisCache.DeleteCache(api.api_key);

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteById(int id, int? user_id)
        {
            var api = await _dbContext.API.FirstOrDefaultAsync(a => a.api_id == id && a.user_id == user_id) ??
                throw new ApiException(ExceptionApiMessages.ApiNotFound);

            await _redisCache.DeleteCache(api.api_key);

            _dbContext.Remove(api);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteByName(string apiKey, int? user_id)
        {
            var api = await _dbContext.API.FirstOrDefaultAsync(a => a.api_key == apiKey && a.user_id == user_id) ??
                throw new ApiException(ExceptionApiMessages.ApiNotFound);

            await _redisCache.DeleteCache(api.api_key);

            _dbContext.Remove(api);
            await _dbContext.SaveChangesAsync();
        }
    }
}
