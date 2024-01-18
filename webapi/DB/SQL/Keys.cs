using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization.Exceptions;
using webapi.Models;
using webapi.Services;

namespace webapi.DB.SQL
{
    public class Keys : ICreate<KeyModel>, IRead<KeyModel>
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IGenerateKey _generateKey;
        private readonly IEncryptKey _encrypt;
        private readonly IConfiguration _configuration;
        private readonly byte[] secretKey;

        public Keys(
            FileCryptDbContext dbContext,
            IGenerateKey generateKey,
            IEncryptKey encrypt,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _generateKey = generateKey;
            _encrypt = encrypt;
            _configuration = configuration;
            secretKey = Convert.FromBase64String(_configuration[App.ENCRYPTION_KEY]!);
        }

        public async Task Create(KeyModel keyModel)
        {
            var user = await _dbContext.Keys.FirstOrDefaultAsync(u => u.user_id == keyModel.user_id);
            if (user == null)
            {
                var privateKey = await _encrypt.EncryptionKeyAsync(_generateKey.GenerateKey(), secretKey);
                keyModel.private_key = privateKey;

                await _dbContext.AddAsync(keyModel);
                await _dbContext.SaveChangesAsync();
            }
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
    }
}
