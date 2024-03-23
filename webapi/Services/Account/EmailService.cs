using Newtonsoft.Json;
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
    public sealed class EmailService(
        IDatabaseTransaction transaction,
        IRepository<UserModel> userRepository,
        IRepository<NotificationModel> notificationRepository,
        IValidation validation,
        IUserInfo userInfo,
        IRedisCache redisCache) : ITransaction<UserModel>, IDataManagement, IValidator
    {
        public async Task CreateTransaction(UserModel user, object? parameter = null)
        {
            try
            {
                var email = (string)parameter;
                if (email is null)
                    throw new EntityNotUpdatedException();

                user.email = email;
                await userRepository.Update(user);

                await notificationRepository.Add(new NotificationModel
                {
                    message_header = NotificationMessage.AUTH_EMAIL_CHANGED_HEADER,
                    message = NotificationMessage.AUTH_EMAIL_CHANGED_BODY,
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    user_id = userInfo.UserId
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
            catch (OperationCanceledException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }

        public async Task DeleteData(int id)
        {
            await redisCache.DeleteCache($"EmailController_Email#{id}");
            await redisCache.DeleteCache($"EmailController_ConfirmationCode_OldEmail#{id}");
            await redisCache.DeleteCache($"EmailController_ConfirmationCode_NewEmail#{id}");

            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{id}");
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{id}");
        }

        public async Task<object> GetData(string key)
        {
            var data = await redisCache.GetCachedData(key);
            if (data is not null)
                return JsonConvert.DeserializeObject<object>(data);
            else
                return null;
        }

        public async Task SetData(string key, object data) => await redisCache.CacheData(key, data, TimeSpan.FromMinutes(10));

        public bool IsValid(object data, object parameter = null) => data is int v && validation.IsSixDigit(v) && parameter.Equals(data);
    }
}
