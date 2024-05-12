using application.Abstractions.TP_Services;
using application.Helpers;
using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using domain.Specifications;
using domain.Specifications.By_Relation_Specifications;
using System.Text.RegularExpressions;

namespace application.Helper_Services.Account
{
    public interface IRecoveryHelper
    {
        public Task RecoveryTransaction(UserModel user, string token, string password);
        public Task CreateTokenTransaction(UserModel user, string token);
    }

    public class RecoveryHelper(
        IRepository<UserModel> userRepository,
        IRepository<NotificationModel> notificationRepository,
        IRepository<LinkModel> linkRepository,
        IRepository<TokenModel> tokenRepository,
        IHashUtility hashUtility,
        IDatabaseTransaction dbTransaction) : IRecoveryHelper, IValidator
    {
        public bool IsValid(object data, object? parameter = null) => Regex.IsMatch((string)data, RegularEx.Password);

        public async Task RecoveryTransaction(UserModel user, string token, string password)
        {
            using var transaction = await dbTransaction.BeginAsync();
            try
            {
                user.password = hashUtility.Hash(password);
                await userRepository.Update(user);

                await notificationRepository.Add(new NotificationModel
                {
                    message_header = NotificationMessage.AUTH_PASSWORD_CHANGED_HEADER,
                    message = NotificationMessage.AUTH_PASSWORD_CHANGED_BODY,
                    priority = (int)Priority.Security,
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    user_id = user.id
                });

                await linkRepository.DeleteByFilter(new RecoveryTokenByTokenSpec(token));

                var tokens = (await tokenRepository.GetAll(new RefreshTokensByRelationSpec(user.id))).Select(x => x.token_id);
                await tokenRepository.DeleteMany(tokens);

                await dbTransaction.CommitAsync(transaction);
            }
            catch (EntityException)
            {
                await dbTransaction.RollbackAsync(transaction);
                throw;
            }
        }

        public async Task CreateTokenTransaction(UserModel user, string token)
        {
            using var transaction = await dbTransaction.BeginAsync();
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
                    priority = (int)Priority.Security,
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    user_id = user.id
                });

                await dbTransaction.CommitAsync(transaction);
            }
            catch (EntityException)
            {
                await dbTransaction.RollbackAsync(transaction);
                throw;
            }
        }
    }
}
