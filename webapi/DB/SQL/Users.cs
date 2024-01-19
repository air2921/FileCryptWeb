using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Localization.Exceptions;
using webapi.Models;
using webapi.Services;

namespace webapi.DB.SQL
{
    public class Users : ICreate<UserModel>, IDelete<UserModel>, IUpdate<UserModel>, IRead<UserModel>
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ICreate<KeyModel> _keyCreate;
        private readonly ICreate<TokenModel> _tokenCreate;
        private readonly IGenerateKey _generateKey;
        private readonly IEncryptKey _encrypt;
        private readonly byte[] secretKey;

        public Users(
            FileCryptDbContext dbContext,
            IConfiguration configuration,
            ICreate<KeyModel> keyCreate,
            ICreate<TokenModel> tokenCreate,
            IGenerateKey generateKey,
            IEncryptKey encrypt)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _keyCreate = keyCreate;
            _tokenCreate = tokenCreate;
            _generateKey = generateKey;
            _encrypt = encrypt;
            secretKey = Convert.FromBase64String(_configuration[App.ENCRYPTION_KEY]!);
        }

        public async Task Create(UserModel userModel)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await _dbContext.Users.AddAsync(userModel);
                await _dbContext.SaveChangesAsync();
                var keyModel = new KeyModel { user_id = userModel.id, private_key = await _encrypt.EncryptionKeyAsync(_generateKey.GenerateKey(), secretKey) };
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

        public async Task DeleteById(int id, int? user_id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.id == id) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserModel> ReadById(int id, bool? byForeign)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.id == id) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);
        }

        public async Task<IEnumerable<UserModel>> ReadAll(int? user_id, int skip, int count)
        {
            return await _dbContext.Users
                .Skip(skip)
                .Take(count)
                .ToListAsync();
        }

        public async Task Update(UserModel userModel, bool? byForeign)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.id == userModel.id) ??
                throw new UserException(AccountErrorMessage.UserNotFound);

            if (userModel.username is not null)
                user.username = userModel.username;

            if (userModel.email is not null)
                user.email = userModel.email;

            if (userModel.password is not null)
                user.password = userModel.password;

            if (userModel.role is not null)
                user.role = userModel.role;

            if (userModel.is_blocked is not null)
                user.is_blocked = userModel.is_blocked;

            if (userModel.is_2fa_enabled is not null)
                user.is_2fa_enabled = userModel.is_2fa_enabled;

            await _dbContext.SaveChangesAsync();
        }
    }
}
