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
    public class RegistrationService : ITransaction<UserObject>, IDataManagement, IValidator
    {
        private readonly IDatabaseTransaction _transaction;
        private readonly IConfiguration _configuration;
        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<KeyModel> _keyRepository;
        private readonly IRedisCache _redisCache;
        private readonly IGenerate _generate;
        private readonly IPasswordManager _passwordManager;
        private readonly ICypherKey _encrypt;
        private readonly byte[] secretKey;

        public RegistrationService(
            IDatabaseTransaction transaction,
            IConfiguration configuration,
            IRepository<UserModel> userRepository,
            IRepository<KeyModel> keyRepository,
            IRedisCache redisCache,
            IGenerate generate,
            IPasswordManager passwordManager,
            [FromKeyedServices("Encrypt")] ICypherKey encrypt)
        {
            _transaction = transaction;
            _configuration = configuration;
            _userRepository = userRepository;
            _keyRepository = keyRepository;
            _redisCache = redisCache;
            _generate = generate;
            _passwordManager = passwordManager;
            _encrypt = encrypt;
            secretKey = Convert.FromBase64String(_configuration[App.ENCRYPTION_KEY]!);
        }

        public async Task CreateTransaction(UserObject user, object? parameter = null)
        {
            try
            {
                var id = await _userRepository.Add(new UserModel
                {
                    email = user.Email,
                    password = user.Password,
                    username = user.Username,
                    role = user.Role,
                    is_2fa_enabled = user.Flag2Fa,
                    is_blocked = false
                }, e => e.id);

                await _keyRepository.Add(new KeyModel
                {
                    user_id = id,
                    private_key = await _encrypt.CypherKeyAsync(_generate.GenerateKey(), secretKey)
                });

                await _transaction.CommitAsync();
            }
            catch (EntityNotCreatedException)
            {
                await _transaction.RollbackAsync();
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
            }
        }

        public Task DeleteData(int id) => throw new NotImplementedException();

        public async Task<object> GetData(string key)
        {
            var userObject = await _redisCache.GetCachedData(key);
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

            user.Password = _passwordManager.HashingPassword(user.Password);
            user.Code = _passwordManager.HashingPassword(user.Code);

            await _redisCache.CacheData(key, user, TimeSpan.FromMinutes(10));
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
