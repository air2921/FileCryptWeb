using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.DB.SQL.Tokens
{
    public class CreateToken : ICreate<TokenModel>
    {
        private readonly FileCryptDbContext _dbContext;

        public CreateToken(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Create(TokenModel tokenModel)
        {
            await _dbContext.AddAsync(tokenModel);
            await _dbContext.SaveChangesAsync();
        }
    }
}
