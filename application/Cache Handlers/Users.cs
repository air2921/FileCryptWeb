using Newtonsoft.Json;
using domain.Abstractions.Data;
using domain.Models;
using Microsoft.Extensions.Logging;
using domain.Specifications;
using domain.Exceptions;
using application.Helpers.Localization;

namespace application.Cache_Handlers
{
    public class Users(
        IRepository<UserModel> userRepository,
        IRedisCache redisCache,
        ILogger<Users> logger) : ICacheHandler<UserModel>
    {
        public async Task<UserModel> CacheAndGet(object dataObject)
        {
            try
            {
                var userObj = dataObject as UserObject ?? throw new FormatException(Message.ERROR);
                var user = new UserModel();
                var cache = await redisCache.GetCachedData(userObj.CacheKey);
                if (cache is null)
                {
                    user = await userRepository.GetById(userObj.UserId);
                    if (user is null)
                        return null;

                    user.password = string.Empty;
                    user.last_time_password_modified = userObj.IsOwner ? user.last_time_password_modified : DateTime.UtcNow;
                    user.email = userObj.IsOwner ? user.email : string.Empty;

                    await redisCache.CacheData(userObj.CacheKey, user, TimeSpan.FromMinutes(10));
                    return user;
                }

                user = JsonConvert.DeserializeObject<UserModel>(cache);
                if (user is null)
                    return null;

                user.last_time_password_modified = userObj.IsOwner ? user.last_time_password_modified : DateTime.UtcNow;
                user.email = userObj.IsOwner ? user.email : string.Empty;
                return user;
            }
            catch (EntityException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(Users));
                throw new FormatException(Message.ERROR);
            }
        }

        public async Task<IEnumerable<UserModel>> CacheAndGetRange(object dataObject)
        {
            try
            {
                var userObj = dataObject as UserRangeObject ?? throw new FormatException(Message.ERROR);
                var users = new List<UserModel>();
                var cache = await redisCache.GetCachedData(userObj.CacheKey);
                if (cache is null)
                {
                    users = (List<UserModel>)await userRepository.GetAll(new UsersByUsernameSpec(userObj.Username));
                    foreach (var user in users)
                    {
                        user.password = string.Empty;
                        user.email = string.Empty;
                    }

                    await redisCache.CacheData(userObj.CacheKey, users, TimeSpan.FromMinutes(3));
                    return users;
                }

                users = JsonConvert.DeserializeObject<List<UserModel>>(cache);
                if (users is not null)
                    return users;
                else
                    throw new FormatException(Message.ERROR);
            }
            catch (EntityException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(Users));
                throw new FormatException(Message.ERROR);
            }
        }
    }

    public record class UserObject(string CacheKey, int UserId, bool IsOwner);
    public record class UserRangeObject(string CacheKey, string Username);
}
