using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.DB.SQL.Users
{
    public class CreateUser : ICreate<UserModel>
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly ICreate<KeyModel> _keyCreate;
        private readonly ICreate<TokenModel> _tokenCreate;

        public CreateUser(FileCryptDbContext dbContext, ICreate<KeyModel> keyCreate, ICreate<TokenModel> tokenCreate)
        {
            _dbContext = dbContext;
            _keyCreate = keyCreate;
            _tokenCreate = tokenCreate;
        }

        public async Task Create(UserModel userModel)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await _dbContext.Users.AddAsync(userModel);
                await _dbContext.SaveChangesAsync();

                var keyModel = new KeyModel { user_id = userModel.id };
                var tokenModel = new TokenModel { user_id = userModel.id };

                await _keyCreate.Create(keyModel);
                await _tokenCreate.Create(tokenModel);

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
