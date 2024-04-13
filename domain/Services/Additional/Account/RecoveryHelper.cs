using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Helpers;
using domain.Localization;
using domain.Models;
using domain.Services.Abstractions;
using domain.Specifications;
using domain.Specifications.By_Relation_Specifications;
using services.Abstractions;
using System.Text.RegularExpressions;

namespace domain.Services.Additional.Account
{
    public interface IRecoveryHelper
    {
        public Task RecoveryTransaction(UserModel user, string token, string password);
        public Task CreateTokenTransaction(UserModel user, string token);
    }

    public class RecoveryHelper(
        IDatabaseTransaction transaction,
        IRepository<UserModel> userRepository,
        IRepository<NotificationModel> notificationRepository,
        IRepository<LinkModel> linkRepository,
        IRepository<TokenModel> tokenRepository,
        IPasswordManager passwordManager) : IRecoveryHelper, IValidator
    {
        public bool IsValid(object data, object? parameter = null) => Regex.IsMatch((string)data, Validation.Password);

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

                await linkRepository.DeleteByFilter(new RecoveryTokenByTokenSpec(token));

                var tokens = (await tokenRepository.GetAll(new RefreshTokensByRelationSpec(user.id))).Select(x => x.token_id);
                await tokenRepository.DeleteMany(tokens);

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
    }
}
