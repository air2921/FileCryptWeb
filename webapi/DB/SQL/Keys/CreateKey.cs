using Microsoft.EntityFrameworkCore;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.DB.SQL.Keys
{
    public class CreateKey : ICreate<KeyModel>
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IGenerateKey _generateKey;
        private readonly IEncryptKey _encrypt;
        private readonly IConfiguration _configuration;
        private readonly byte[] secretKey;

        public CreateKey(
            FileCryptDbContext dbContext,
            IGenerateKey generateKey,
            IEncryptKey encrypt,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _generateKey = generateKey;
            _encrypt = encrypt;
            _configuration = configuration;
            secretKey = Convert.FromBase64String(_configuration["FileCryptKey"]!);
        }

        public async Task Create(KeyModel keyModel)
        {
            var user = await _dbContext.Keys.FirstOrDefaultAsync(u => u.user_id == keyModel.user_id);
            if (user == null)
            {
                var tempKey = _generateKey.GenerateKey();
                var privateKey = await _encrypt.EncryptionKeyAsync(tempKey, secretKey);
                keyModel.private_key = privateKey;

                await _dbContext.AddAsync(keyModel);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
