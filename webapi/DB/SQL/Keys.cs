using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.SQL;
using webapi.Localization.Exceptions;
using webapi.Models;
using webapi.Services;

namespace webapi.DB.SQL
{
    public class Keys : ICreate<KeyModel>, IRead<KeyModel>, IUpdate<KeyModel>
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IEncryptKey _encrypt;
        private readonly byte[] secretKey;

        public Keys(FileCryptDbContext dbContext, IConfiguration configuration, IEncryptKey encrypt)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _encrypt = encrypt;
            secretKey = Convert.FromBase64String(_configuration[App.ENCRYPTION_KEY]!);
        }

        public async Task Create(KeyModel keyModel)
        {
            await _dbContext.AddAsync(keyModel);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<KeyModel> ReadById(int id, bool? byForeign)
        {
            if(byForeign == false)
            {
                return await _dbContext.Keys.FirstOrDefaultAsync(k => k.key_id == id) ??
                    throw new UserException(ExceptionUserMessages.UserNotFound);
            }

            return await _dbContext.Keys.FirstOrDefaultAsync(k => k.user_id == id) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);
        }

        public async Task<IEnumerable<KeyModel>> ReadAll(int? user_id, int skip, int count)
        {
            return await _dbContext.Keys
                .Skip(skip)
                .Take(count)
                .ToListAsync();
        }

        public async Task Update(KeyModel keyModel, bool? byForeign)
        {
            var keys = byForeign == true
                ? await _dbContext.Keys.FirstOrDefaultAsync(k => k.user_id == keyModel.user_id)
                : await _dbContext.Keys.FirstOrDefaultAsync(k => k.key_id == keyModel.key_id);

            if (keys is null)
                throw new UserException(ExceptionUserMessages.UserNotFound);

            keys.private_key = await _encrypt.EncryptionKeyAsync(keyModel.private_key, secretKey);
            keys.internal_key = keyModel.internal_key is not null ? await _encrypt.EncryptionKeyAsync(keyModel.internal_key, secretKey) : keys.internal_key;
            keys.received_key = keyModel.received_key;

            await _dbContext.SaveChangesAsync();
        }
    }
}
