using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL.Tokens;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL.Tokens
{
    public class ReadToken : IReadToken
    {
        private readonly FileCryptDbContext _dbContext;

        public ReadToken(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TokenModel> ReadRefresh(int id)
        {
            var token = await _dbContext.Tokens.FirstOrDefaultAsync(t => t.user_id == id) ??
                throw new TokenException(ExceptionUserMessages.UserNotFound);

            return token;
        }

        public async Task<TokenModel> ReadRefresh(TokenModel tokenModel)
        {
            var token = await _dbContext.Tokens.FirstOrDefaultAsync(t => t.refresh_token == tokenModel.refresh_token) ??
                throw new TokenException(ExceptionUserMessages.UserNotFound);

            return token;
        }

        public async Task<int[]> ReadSuspectRefresh()
        {
            var tokens = await _dbContext.Tokens.Where(t => t.refresh_token.EndsWith("==")).Select(t => t.user_id).ToArrayAsync() ??
                throw new TokenException(ExceptionTokenMessages.NoOneSuspectRefresh);

            return tokens;
        }
    }
}
