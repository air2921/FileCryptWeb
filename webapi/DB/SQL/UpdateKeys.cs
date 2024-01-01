using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
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
    public class UpdateKeys : IUpdateKeys
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IValidation _validation;
        private readonly IConfiguration _configuration;
        private readonly IEncryptKey _encrypt;
        private readonly byte[] secretKey;

        public UpdateKeys(
            FileCryptDbContext dbContext,
            IValidation validation,
            IConfiguration configuration,
            IEncryptKey encrypt)
        {
            _dbContext = dbContext;
            _validation = validation;
            _configuration = configuration;
            _encrypt = encrypt;
            secretKey = Convert.FromBase64String(_configuration[App.appKey]!);
        }

        public async Task CleanReceivedInternalKey(int id)
        {
            var existingUser = await _dbContext.Keys.FirstOrDefaultAsync(u => u.user_id == id) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);

            existingUser.received_key = null;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdatePersonalInternalKey(KeyModel keyModel)
        {
            if (string.IsNullOrEmpty(keyModel.internal_key))
                throw new ArgumentException(ErrorMessage.InvalidKey);

            if (Regex.IsMatch(keyModel.internal_key, Validation.EncryptionKey) && _validation.IsBase64String(keyModel.internal_key))
            {
                var existingUser = await _dbContext.Keys.FirstOrDefaultAsync(u => u.user_id == keyModel.user_id) ??
                    throw new UserException(ExceptionUserMessages.UserNotFound);

                var internalKey = await _encrypt.EncryptionKeyAsync(keyModel.internal_key, secretKey);
                existingUser.internal_key = internalKey;
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException(ErrorMessage.InvalidKey);
            }
        }

        public async Task UpdatePrivateKey(KeyModel keyModel)
        {
            if (string.IsNullOrEmpty(keyModel.private_key))
                throw new ArgumentException(ErrorMessage.InvalidKey);

            if (Regex.IsMatch(keyModel.private_key, Validation.EncryptionKey) && _validation.IsBase64String(keyModel.private_key) == true)
            {
                var existingUser = await _dbContext.Keys.FirstOrDefaultAsync(u => u.user_id == keyModel.user_id) ??
                    throw new UserException(ExceptionUserMessages.UserNotFound);

                existingUser.private_key = await _encrypt.EncryptionKeyAsync(keyModel.private_key, secretKey);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException(ErrorMessage.InvalidKey);
            }
        }
    }
}
