using application.Abstractions.Services.Inner;
using application.DTO.Inner;
using application.Helpers;
using application.Helpers.Localization;
using application.Services.Abstractions;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using domain.Specifications;
using domain.Specifications.By_Relation_Specifications;
using Newtonsoft.Json;

namespace application.Services.Additional.Account
{
    public interface ISessionHelper
    {
        public Task<Response> RevokeToken(string token);
        public Task<Response> GenerateCredentials(UserModel user, string refresh);
    }

    public class SessionHelper(
        IDatabaseTransaction transaction,
        IRepository<TokenModel> tokenRepository,
        IRepository<NotificationModel> notificationRepository,
        IRedisCache redisCache,
        ITokenComparator tokenComparator) : ISessionHelper, IDataManagement
    {
        private async Task LoginTransaction(UserModel user, string refreshToken)
        {
            try
            {
                await tokenRepository.Add(new TokenModel
                {
                    user_id = user.id,
                    refresh_token = refreshToken,
                    expiry_date = DateTime.UtcNow + ImmutableData.RefreshExpiry
                });

                await notificationRepository.Add(new NotificationModel
                {
                    message_header = NotificationMessage.AUTH_NEW_LOGIN_HEADER,
                    message = NotificationMessage.AUTH_NEW_LOGIN_BODY,
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    user_id = user.id
                });

                await DeleteExpiredTokens(user.id);

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

        private async Task DeleteExpiredTokens(int id)
        {
            try
            {
                var tokens = (await tokenRepository.GetAll(new RefreshTokenByTokenAndExpiresSpec(id, DateTime.UtcNow)))
                    .Select(x => x.token_id);

                await tokenRepository.DeleteMany(tokens);
            }
            catch (EntityException)
            {
                throw;
            }
        }

        private CredentialsDTO GetCredentials(UserModel user, string token)
        {
            var jwt = tokenComparator.CreateJWT(new JwtDTO
            {
                UserId = user.id,
                Email = user.email,
                Username = user.username,
                Role = user.role,
                Expires = ImmutableData.JwtExpiry
            });

            return new CredentialsDTO
            {
                Jwt = jwt,
                Refresh = token,
                IsAuth = true,
                Role = user.role,
                Id = user.id.ToString(),
                Username = user.username
            };
        }

        public async Task<Response> GenerateCredentials(UserModel user, string refresh)
        {
            try
            {
                await LoginTransaction(user, refresh);
                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{user.id}");

                return new Response { Status = 200, ObjectData = GetCredentials(user, refresh) };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> RevokeToken(string token)
        {
            try
            {
                var tokenModel = await tokenRepository.DeleteByFilter(new RefreshTokenByTokenSpec(token));
                if (tokenModel is not null)
                    await DeleteExpiredTokens(tokenModel.user_id);

                return new Response { Status = 204 };
            }
            catch (EntityException)
            {
                throw;
            }
        }

        public async Task SetData(string key, object data) => await redisCache.CacheData(key, data, TimeSpan.FromMinutes(8));

        public async Task<object> GetData(string key)
        {
            var user = await redisCache.GetCachedData(key);
            if (user is not null)
                return JsonConvert.DeserializeObject<UserContextDTO>(user);
            else
                return null;
        }

        public Task DeleteData(int id, object? parameter = null) => throw new NotImplementedException();
    }
}
