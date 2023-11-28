using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL
{
    public class Users : ICreate<UserModel>, IDelete<UserModel>, IUpdate<UserModel>, IRead<UserModel>
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly ICreate<KeyModel> _keyCreate;
        private readonly ICreate<TokenModel> _tokenCreate;

        public Users(FileCryptDbContext dbContext, ICreate<KeyModel> keyCreate, ICreate<TokenModel> tokenCreate)
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

        public async Task DeleteById(int id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.id == id) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserModel> ReadById(int id, bool? byForeign)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.id == id) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);

            return user;
        }

        public async Task<IEnumerable<UserModel>> ReadAll()
        {
            var users = await _dbContext.Users.ToListAsync() ??
                throw new UserException(ExceptionUserMessages.NoOneUserNotFound);

            return users;
        }

        public async Task Update(UserModel userModel, bool? byForeign)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.id == userModel.id);

            if (userModel.username is not null)
                user.username = userModel.username;

            if (userModel.email is not null)
                user.email = userModel.email;

            if (userModel.password_hash is not null)
                user.password_hash = userModel.password_hash;

            if (userModel.role is not null)
                user.role = userModel.password_hash;

            await _dbContext.SaveChangesAsync();
        }
    }
}
