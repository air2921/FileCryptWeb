using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL
{
    public class Tokens : ICreate<TokenModel>, IRead<TokenModel>, IUpdate<TokenModel>
    {
        private readonly FileCryptDbContext _dbContext;

        public Tokens(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Create(TokenModel tokenModel)
        {
            await _dbContext.AddAsync(tokenModel);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<TokenModel> ReadById(int id, bool? byForeign)
        {
            var token = _dbContext.Tokens.AsQueryable();

            if (byForeign == false)
            {
                return await token.FirstOrDefaultAsync(t => t.token_id == id) ??
                    throw new TokenException(ExceptionTokenMessages.RefreshNotFound);
            }

            return await token.FirstOrDefaultAsync(t => t.user_id == id) ??
                throw new TokenException(ExceptionTokenMessages.RefreshNotFound);
        }

        public async Task<IEnumerable<TokenModel>> ReadAll()
        {
            return await _dbContext.Tokens.ToListAsync() ??
                throw new TokenException(ExceptionTokenMessages.NoOneRefreshNotFound);
        }

        public async Task Update(TokenModel tokenModel, bool? byForeign)
        {
            if(byForeign == false)
            {
                _dbContext.Update(tokenModel);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                var existingToken = await _dbContext.Tokens.FirstOrDefaultAsync(t => t.user_id == tokenModel.user_id) ??
                    throw new TokenException(ExceptionTokenMessages.RefreshNotFound);

                existingToken.expiry_date = tokenModel.expiry_date;
                existingToken.refresh_token = tokenModel.refresh_token;
            }
        }
    }
}
