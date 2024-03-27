using Newtonsoft.Json;
using System.Text.RegularExpressions;
using webapi.Attributes;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Models;

namespace webapi.Services.Account
{
    public class RegistrationService(
        IDatabaseTransaction transaction,
        IConfiguration configuration,
        IRepository<UserModel> userRepository,
        IRepository<KeyModel> keyRepository,
        IRedisCache redisCache,
        IGenerate generate,
        IPasswordManager passwordManager,
        [FromKeyedServices("Encrypt")] ICypherKey encrypt) : ITransaction<UserObject>, IDataManagement, IValidator
    {
        private readonly byte[] secretKey = Convert.FromBase64String(configuration[App.ENCRYPTION_KEY]!);

        public async Task CreateTransaction(UserObject user, object? parameter = null)
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
                    private_key = await encrypt.CypherKeyAsync(generate.GenerateKey(), secretKey)
                });

                await transaction.CommitAsync();
            }
            catch (EntityNotCreatedException)
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
                return JsonConvert.DeserializeObject<UserObject>(userObject);
            else
                return null;
        }

        public async Task SetData(string key, object data)
        {
            if (data is not UserObject)
                throw new ArgumentException();

            var user = data as UserObject;
            if (user is null)
                throw new ArgumentException();

            user.Password = passwordManager.HashingPassword(user.Password);
            user.Code = passwordManager.HashingPassword(user.Code);

            await redisCache.CacheData(key, user, TimeSpan.FromMinutes(10));
        }

        public bool IsValid(object data, object parameter = null)
        {
            if (data is not RegisterDTO)
                return false;

            var user = data as RegisterDTO;
            if (user is null)
                return false;

            bool isValidUsername = Regex.IsMatch(user.username, Validation.Username);
            bool isValidPassword = Regex.IsMatch(user.password, Validation.Password);

            return isValidUsername && isValidPassword;
        }
    }

    [AuxiliaryObject]
    public class UserObject
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public bool Flag2Fa { get; set; }
        public string Code { get; set; }
    }
}
