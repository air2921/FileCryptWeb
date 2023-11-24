using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL.API
{
    public class DeleteAPI : IDelete<ApiModel>, IDeleteByName<ApiModel>
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IRedisCache _redisCache;

        public DeleteAPI(FileCryptDbContext dbContext, IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _redisCache = redisCache;
        }

        public async Task DeleteById(int id)
        {
            var api = await _dbContext.API.FirstOrDefaultAsync(a => a.user_id == id) ??
                throw new ApiException(ExceptionApiMessages.UserApiNotFound);

            await _redisCache.DeleteCache(api.api_key);

            _dbContext.Remove(api);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteByName(string apiKey)
        {
            var api = await _dbContext.API.FirstOrDefaultAsync(a => a.api_key == apiKey) ??
                throw new ApiException(ExceptionApiMessages.UserApiNotFound);

            await _redisCache.DeleteCache(api.api_key);

            _dbContext.Remove(api);
            await _dbContext.SaveChangesAsync();
        }
    }
}
