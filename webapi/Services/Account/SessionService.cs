using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using webapi.Attributes;
using webapi.DB.Abstractions;
using webapi.DB.Ef.Specifications;
using webapi.DB.Ef.Specifications.By_Relation_Specifications;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Abstractions;

namespace webapi.Services.Account
{
    public interface ISessionHelpers
    {
        public Task<IActionResult> CreateTokens(UserModel user, HttpContext context);
        public Task RevokeToken(HttpContext context);
    }

    public sealed class SessionService(
        IDatabaseTransaction transaction,
        IRepository<TokenModel> tokenRepository,
        IRepository<NotificationModel> notificationRepository,
        IRedisCache redisCache,
        ITokenService tokenService) : ControllerBase, IDataManagement, ISessionHelpers
    {
        private void CookieAppend(UserModel user, HttpContext context, string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                MaxAge = ImmutableData.JwtExpiry,
                Secure = true,
                HttpOnly = false,
                SameSite = SameSiteMode.None,
                IsEssential = false
            };

            context.Response.Cookies.Append(ImmutableData.JWT_COOKIE_KEY, tokenService.GenerateJwtToken(user, ImmutableData.JwtExpiry), tokenService.SetCookieOptions(ImmutableData.JwtExpiry));
            context.Response.Cookies.Append(ImmutableData.REFRESH_COOKIE_KEY, refreshToken, tokenService.SetCookieOptions(ImmutableData.RefreshExpiry));
            context.Response.Cookies.Append(ImmutableData.IS_AUTHORIZED, true.ToString(), cookieOptions);
            context.Response.Cookies.Append(ImmutableData.USER_ID_COOKIE_KEY, user.id.ToString(), cookieOptions);
            context.Response.Cookies.Append(ImmutableData.USERNAME_COOKIE_KEY, user.username, cookieOptions);
            context.Response.Cookies.Append(ImmutableData.ROLE_COOKIE_KEY, user.role, cookieOptions);
        }

        [Helper]
        private async Task LoginTransaction(UserModel user, string refreshToken)
        {
            try
            {
                await tokenRepository.Add(new TokenModel
                {
                    user_id = user.id,
                    refresh_token = tokenService.HashingToken(refreshToken),
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

        [Helper]
        [NonAction]
        public async Task<IActionResult> CreateTokens(UserModel user, HttpContext context)
        {
            try
            {
                string refreshToken = tokenService.GenerateRefreshToken();
                await LoginTransaction(user, refreshToken);
                CookieAppend(user, context, refreshToken);

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{user.id}");

                return StatusCode(200);
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Helper]
        public async Task RevokeToken(HttpContext context)
        {
            try
            {
                var refresh = context.Request.Cookies[ImmutableData.REFRESH_COOKIE_KEY];
                if (refresh is null)
                    return;

                var token = await tokenRepository.DeleteByFilter(new RefreshTokenByTokenSpec(tokenService.HashingToken(refresh)));
            }
            catch (EntityNotDeletedException)
            {
                throw;
            }
        }

        [Helper]
        public async Task SetData(string key, object data) => await redisCache.CacheData(key, data, TimeSpan.FromMinutes(8));

        [Helper]
        public async Task<object> GetData(string key)
        {
            var user = await redisCache.GetCachedData(key);
            if (user is not null)
                return JsonConvert.DeserializeObject<UserContextObject>(user);
            else
                return null;
        }

        public Task DeleteData(int id, object? parameter = null) => throw new NotImplementedException();
    }

    [AuxiliaryObject]
    public class UserContextObject
    {
        public int UserId { get; set; }
        public string Code { get; set; }
    }
}
