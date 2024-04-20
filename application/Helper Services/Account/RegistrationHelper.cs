using application.Abstractions.Services.TP_Services;
using application.DTO.Inner;
using application.DTO.Outer;
using application.Helpers;
using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace application.Helper_Services.Account
{
    public class RegistrationHelper(
        IDatabaseTransaction transaction,
        IRepository<UserModel> userRepository,
        IRedisCache redisCache,
        IHashUtility hashUtility) : ITransaction<UserDTO>, IDataManagement, IValidator
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
            if (data is not UserDTO user)
                throw new ArgumentException(Message.ERROR);

            user.Password = hashUtility.Hash(user.Password);
            user.Code = hashUtility.Hash(user.Code);

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
