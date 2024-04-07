﻿using System.Security.Cryptography;
using System.Text.RegularExpressions;
using webapi.Attributes;
using webapi.Cryptography.Abstractions;
using webapi.DB.Abstractions;
using webapi.DB.Ef.Specifications.By_Relation_Specifications;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Models;
using webapi.Services.Abstractions;

namespace webapi.Services.Core
{
    public interface IKeyHelper
    {
        Task UpdateKey(string key, int id, FileType type);
    }

    public class KeyService(
        IRepository<KeyModel> keyRepository,
        [FromKeyedServices("Encrypt")] ICypherKey cypher,
        IRedisCache redisCache,
        IValidation validation,
        IConfiguration configuration) : IValidator, IDataManagement, IKeyHelper
    {
        private readonly byte[] secretKey = Convert.FromBase64String(configuration[App.ENCRYPTION_KEY]!);

        public bool IsValid(object data, object parameter = null)
        {
            var key = data as string;
            return !string.IsNullOrWhiteSpace(key) && validation.IsBase64String(key) && Regex.IsMatch(key, Validation.EncryptionKey);
        }

        public Task SetData(string key, object data) => throw new NotImplementedException();
        public Task<object> GetData(string key) => throw new NotImplementedException();

        public async Task DeleteData(int id, object? paramater = null)
        {
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.KEYS_PREFIX}{id}");
            await redisCache.DeleteCache((string)paramater!);
        }

        public async Task UpdateKey(string key, int id, FileType type)
        {
            try
            {
                var keys = await keyRepository.GetByFilter(new KeysByRelationSpec(id));

                if (type.Equals(FileType.Private))
                    keys.private_key = await cypher.CypherKeyAsync(key, secretKey);
                else if (type.Equals(FileType.Internal))
                    keys.internal_key = await cypher.CypherKeyAsync(key, secretKey);
                else
                    throw new EntityNotUpdatedException();

                await keyRepository.Update(keys);
            }
            catch (EntityNotUpdatedException)
            {
                throw;
            }
            catch (CryptographicException)
            {
                throw;
            }
        }
    }
}
