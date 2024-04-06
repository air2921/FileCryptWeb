using System.Text.RegularExpressions;
using webapi.Attributes;
using webapi.DB.Abstractions;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Abstractions;

namespace webapi.Services.Account
{
    public interface IRecoveryHelpers
    {
        public Task RecoveryTransaction(UserModel user, string token, string password);
        public Task CreateTokenTransaction(UserModel user, string token);
    }

    public sealed class RecoveryService(
        IDatabaseTransaction transaction,
        IRepository<UserModel> userRepository,
        IRepository<NotificationModel> notificationRepository,
        IRepository<LinkModel> linkRepository,
        IRepository<TokenModel> tokenRepository,
        IPasswordManager passwordManager) : IValidator, IRecoveryHelpers
    {
        public bool IsValid(object data, object parameter = null) => Regex.IsMatch((string)data, Validation.Password);

        [Helper]
        public async Task RecoveryTransaction(UserModel user, string token, string password)
        {
            try
            {
                user.password = passwordManager.HashingPassword(password);
                await userRepository.Update(user);

                await notificationRepository.Add(new NotificationModel
                {
                    message_header = NotificationMessage.AUTH_PASSWORD_CHANGED_HEADER,
                    message = NotificationMessage.AUTH_PASSWORD_CHANGED_BODY,
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    user_id = user.id
                });

                await linkRepository.DeleteByFilter(query => query.Where(l => l.u_token.Equals(token)));
                var tokens = new List<int>();

                var tokenModels = await tokenRepository.GetAll(query => query.Where(t => t.user_id.Equals(user.id)));
                foreach (var tokenModel in tokenModels)
                    tokens.Add(tokenModel.token_id);

                await tokenRepository.DeleteMany(tokens);

                await transaction.CommitAsync();
            }
            catch (EntityNotUpdatedException)
            {
                await transaction.RollbackAsync();
                throw;
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
            finally
            {
                await transaction.DisposeAsync();
            }
        }

        [Helper]
        public async Task CreateTokenTransaction(UserModel user, string token)
        {
            try
            {
                await linkRepository.Add(new LinkModel
                {
                    user_id = user.id,
                    u_token = token,
                    expiry_date = DateTime.UtcNow.AddMinutes(30),
                    created_at = DateTime.UtcNow
                });

                await notificationRepository.Add(new NotificationModel
                {
                    message_header = "Someone trying recovery your account",
                    message = $"Someone trying recovery your account {user.username}#{user.id} at {DateTime.UtcNow}." +
                    $"Unique token was sent on {user.email}",
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
            finally
            {
                await transaction.DisposeAsync();
            }
        }
    }
}
