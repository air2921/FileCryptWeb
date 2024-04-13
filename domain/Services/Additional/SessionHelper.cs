using domain.Abstractions.Data;
using domain.DTO;
using domain.Exceptions;
using domain.Helpers;
using domain.Localization;
using domain.Models;
using domain.Services.Abstractions;
using domain.Specifications;
using domain.Specifications.By_Relation_Specifications;
using Newtonsoft.Json;

namespace domain.Services.Additional
{
    public interface ISessionHelper
    {
        public Task<Response> RevokeToken(string token);
        public Task<Response> GenerateCredentials(UserModel user, string token);
    }

    public class SessionHelper(
        IDatabaseTransaction transaction,
        IRepository<TokenModel> tokenRepository,
        IRepository<NotificationModel> notificationRepository,
        IRedisCache redisCache) : ISessionHelper, IDataManagement
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
            catch (EntityNotCreatedException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (EntityNotDeletedException)
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

        private async Task DeleteExpiredTokens(int id)
        {
            try
            {
                var tokens = (await tokenRepository.GetAll(new RefreshTokenByTokenAndExpiresSpec(id, DateTime.UtcNow)))
                    .Select(x => x.token_id);

                await tokenRepository.DeleteMany(tokens);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (EntityNotDeletedException)
            {
                throw;
            }
        }
        
        private CredentialsDTO GetCredentials(UserModel user, string token)
        {
            return new CredentialsDTO
            {
                Refresh = token,
                IsAuth = true,
                Role = user.role,
                Id = user.id.ToString(),
                Username = user.username
            };
        }

        public async Task<Response> GenerateCredentials(UserModel user, string token)
        {
            try
            {
                await LoginTransaction(user, token);

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{user.id}");

                return new Response { Status = 200, ObjectData = GetCredentials(user, token) };
            }
            catch (EntityNotCreatedException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (EntityNotDeletedException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (OperationCanceledException ex)
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
            catch (EntityNotDeletedException)
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
