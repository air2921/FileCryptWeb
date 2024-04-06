using System.Text.RegularExpressions;
using webapi.DB.Abstractions;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Abstractions;

namespace webapi.Services.Account
{
    public sealed class UsernameService(
        IRepository<UserModel> userRepository,
        ITokenService tokenService,
        IRedisCache redisCache) : ITransaction<UserModel>, IDataManagement, IValidator
    {
        public async Task CreateTransaction(UserModel user, object? parameter = null)
        {
            try
            {
                var username = (string)parameter;
                if (username is null)
                    throw new EntityNotUpdatedException(Message.ERROR);

                user.username = username;
                await userRepository.Update(user);
            }
            catch (EntityNotUpdatedException)
            {
                throw;
            }
        }

        public async Task DeleteData(int id, object? parameter = null)
        {
            tokenService.DeleteUserDataSession();
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{id}");
        }

        public Task<object> GetData(string key) => throw new NotImplementedException();

        public Task SetData(string key, object data) => throw new NotImplementedException();

        public bool IsValid(object data, object parameter = null) => Regex.IsMatch((string)data, Validation.Username);
    }
}
