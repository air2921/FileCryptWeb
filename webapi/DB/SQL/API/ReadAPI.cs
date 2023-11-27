using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL.API;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL.API
{
    public class ReadAPI : IReadAPI
    {
        private readonly FileCryptDbContext _dbContext;

        public ReadAPI(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiModel> ReadUserApiSettings(int id)
        {
            var api = await _dbContext.API.FirstOrDefaultAsync(a => a.user_id == id) ??
                throw new ApiException(ExceptionApiMessages.UserApiNotFound);

            return api;
        }

        public async Task<int> ReadUserIdByApiKey(string apiKey)
        {
            var user = await _dbContext.API.FirstOrDefaultAsync(u => u.api_key == apiKey) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);

            return user.user_id;
        }

        public async Task<dynamic> ReadUserByApiKey(string apiKey)
        {
            var user = await _dbContext.API
                .Where(a => a.api_key == apiKey)
                .Join(_dbContext.Users, api => api.user_id, user => user.id, (api, user) => new { api, user })
                .Join(_dbContext.Tokens,
                    joined => joined.user.id,
                    token => token.user_id,
                    (joined, token) => new { joined.api, joined.user, token })
                .Join(_dbContext.Keys,
                joinedKeys => joinedKeys.user.id,
                key => key.user_id,
                (joined, key) => new { joined.user, joined.api, joined.token, key })
                .FirstOrDefaultAsync() ?? throw new ApiException(ExceptionUserMessages.UserNotFound);


            return user;
        }
    }
}
