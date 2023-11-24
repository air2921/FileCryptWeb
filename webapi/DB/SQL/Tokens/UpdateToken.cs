using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL.Tokens;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL.Tokens
{
    public class UpdateToken : IUpdateToken
    {
        private readonly FileCryptDbContext _dbContext;

        public UpdateToken(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public const string USER_ID = "USER_ID";
        public const string TOKEN_VALUE = "TOKEN";

        public async Task UpdateRefreshToken(TokenModel tokenModel, string searchField)
        {
            var tokenQuery = _dbContext.Tokens.AsQueryable();

            switch (searchField)
            {
                case USER_ID:
                    tokenQuery = tokenQuery.Where(t => t.user_id == tokenModel.user_id);
                    break;
                case TOKEN_VALUE:
                    tokenQuery = tokenQuery.Where(t => t.refresh_token == tokenModel.refresh_token);
                    break;
                default:
                    throw new ArgumentException("Invalid search field", nameof(searchField));
            }

            var token = await tokenQuery.FirstOrDefaultAsync() ??
                throw new TokenException(ExceptionUserMessages.UserNotFound);

            token.refresh_token = tokenModel.refresh_token;
            token.expiry_date = tokenModel.expiry_date;

            await _dbContext.SaveChangesAsync();
        }
    }
}
