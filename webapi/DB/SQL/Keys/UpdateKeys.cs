﻿using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using webapi.Exceptions;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL.Keys;
using webapi.Localization.English;
using webapi.Models;
using webapi.Services;

namespace webapi.DB.SQL.Keys
{
    public class UpdateKeys : IUpdateKeys
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IValidation _validation;
        private readonly IGenerateKey _generateKey;
        private readonly IConfiguration _configuration;
        private readonly IEncryptKey _encrypt;
        private readonly byte[] secretKey;

        public UpdateKeys(
            FileCryptDbContext dbContext,
            IValidation validation,
            IGenerateKey generateKey,
            IConfiguration configuration,
            IEncryptKey encrypt)
        {
            _dbContext = dbContext;
            _validation = validation;
            _generateKey = generateKey;
            _configuration = configuration;
            _encrypt = encrypt;
            secretKey = Convert.FromBase64String(_configuration["FileCryptKey"]!);
        }

        public async Task CleanReceivedInternalKey(int id)
        {
            var existingUser = await _dbContext.Keys.FirstOrDefaultAsync(u => u.user_id == id) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);

            existingUser.received_internal_key = null;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdatePersonalInternalKeyToYourOwn(KeyModel keyModel)
        {
            if (string.IsNullOrEmpty(keyModel.person_internal_key))
                throw new ArgumentException(ErrorMessage.InvalidKey);

            if (Regex.IsMatch(keyModel.person_internal_key, Validation.EncryptionKey) && _validation.IsBase64String(keyModel.person_internal_key))
            {
                var existingUser = await _dbContext.Keys.FirstOrDefaultAsync(u => u.user_id == keyModel.user_id) ??
                    throw new UserException(ExceptionUserMessages.UserNotFound);

                var internalKey = await _encrypt.EncryptionKeyAsync(keyModel.person_internal_key, secretKey);
                existingUser.person_internal_key = internalKey;
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException(ErrorMessage.InvalidKey);
            }
        }

        public async Task UpdatePrivateKeyToYourOwn(KeyModel keyModel)
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

        public async Task UpdatePersonalInternalKey(int id)
        {
            var existingUser = await _dbContext.Keys.FirstOrDefaultAsync(u => u.user_id == id) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);

            var internalKey = await _encrypt.EncryptionKeyAsync(_generateKey.GenerateKey(), secretKey);
            existingUser.person_internal_key = internalKey;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdatePrivateKey(int id)
        {
            var existingUser = await _dbContext.Keys.FirstOrDefaultAsync(u => u.user_id == id) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);

            var privateKey = await _encrypt.EncryptionKeyAsync(_generateKey.GenerateKey(), secretKey);
            existingUser.private_key = privateKey;
            await _dbContext.SaveChangesAsync();
        }
    }
}
