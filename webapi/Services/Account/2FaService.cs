using Newtonsoft.Json;
using webapi.Attributes;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Services.Account
{
    public sealed class _2FaService(
        IDatabaseTransaction transaction,
        IRepository<UserModel> userRepository,
        IRepository<NotificationModel> notificationRepository,
        IValidation validation,
        IRedisCache redisCache) : ITransaction<UserModel>, IDataManagement, IValidator
    {
        [Helper]
        public async Task CreateTransaction(UserModel user, object? parameter = null)
        {
            try
            {
                if (parameter is not bool || parameter is null)
                    throw new EntityNotUpdatedException(Message.ERROR);
                bool enable = (bool)parameter;

                if (user.is_2fa_enabled == enable)
                    throw new EntityNotUpdatedException(Message.CONFLICT);

                user.is_2fa_enabled = enable;
                await userRepository.Update(user);

                await notificationRepository.Add(new NotificationModel
                {
                    message_header = NotificationMessage.AUTH_2FA_HEADER,
                    message = enable ? NotificationMessage.AUTH_2FA_ENABLE_BODY : NotificationMessage.AUTH_2FA_DISABLE_BODY,
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    user_id = user.id
                });

                await transaction.CommitAsync();
            }
            catch (EntityNotCreatedException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (EntityNotUpdatedException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }

        [Helper]
        public async Task DeleteData(int id, object? parameter = null)
        {
            await redisCache.DeleteCache($"_2FaController_VerificationCode#{id}");
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{id}");
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{id}");
        }

        [Helper]
        public async Task<object> GetData(string key)
        {
            var code = await redisCache.GetCachedData(key);
            if (code is not null)
                return JsonConvert.DeserializeObject<int>(code);
            else
                return 0;
        }

        [Helper]
        public async Task SetData(string key, object data) => await redisCache.CacheData(key, data, TimeSpan.FromMinutes(10));

        public bool IsValid(object data, object parameter = null) => data is int v && validation.IsSixDigit(v) && parameter.Equals(data);
    }
}
