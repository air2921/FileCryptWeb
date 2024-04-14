using domain.Abstractions.Data;
using domain.Abstractions.Services;
using domain.DTO;
using domain.Exceptions;
using domain.Helpers;
using domain.Localization;
using domain.Models;
using domain.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace domain.Services.Additional.Account
{
    public class RegistrationHelper(
        IDatabaseTransaction transaction,
        IRepository<UserModel> userRepository,
        IRepository<KeyModel> keyRepository,
        IRedisCache redisCache,
        IGenerate generate,
        IPasswordManager passwordManager,
        [FromKeyedServices("Encrypt")] ICypherKey encrypt,
        Secret secret) : ITransaction<UserDTO>, IDataManagement, IValidator
    {
        public async Task CreateTransaction(UserDTO user, object? parameter = null)
        {
            try
            {
                var id = await userRepository.Add(new UserModel
                {
                    email = user.Email,
                    password = user.Password,
                    username = user.Username,
                    role = user.Role,
                    is_2fa_enabled = user.Flag2Fa,
                    is_blocked = false
                }, e => e.id);

                await keyRepository.Add(new KeyModel
                {
                    user_id = id,
                    private_key = await encrypt.CypherKeyAsync(generate.GenerateKey(), secret.Key)
                });

                await transaction.CommitAsync();
            }
            catch (EntityException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }

        public Task DeleteData(int id, object? parameter = null) => throw new NotImplementedException();

        public async Task<object> GetData(string key)
        {
            var userObject = await redisCache.GetCachedData(key);
            if (userObject is not null)
                return JsonConvert.DeserializeObject<UserDTO>(userObject);
            else
                return null;
        }

        public async Task SetData(string key, object data)
        {
            var user = data as UserDTO;
            if (user is null)
                throw new ArgumentException(Message.ERROR);

            user.Password = passwordManager.HashingPassword(user.Password);
            user.Code = passwordManager.HashingPassword(user.Code);

            await redisCache.CacheData(key, user, TimeSpan.FromMinutes(10));
        }

        public bool IsValid(object data, object? parameter = null)
        {
            var user = data as RegisterDTO;
            if (user is null)
                return false;

            bool isValidUsername = Regex.IsMatch(user.Username, Validation.Username);
            bool isValidPassword = Regex.IsMatch(user.Password, Validation.Password);

            return isValidUsername && isValidPassword;
        }
    }
}
