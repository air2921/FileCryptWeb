using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using domain.Specifications.By_Relation_Specifications;

namespace application.Helper_Services.Admin
{
    public class TokenService(
        IRepository<TokenModel> tokenRepository,
        IRepository<NotificationModel> notificationRepository,
        IDatabaseTransaction transaction) : ITransaction<TokenModel>, IValidator
    {
        public async Task CreateTransaction(TokenModel data, object? parameter = null)
        {
            try
            {
                if (!int.TryParse(parameter?.ToString(), out int userId))
                    throw new EntityException("Error when deleting data");

                await tokenRepository.DeleteMany((await tokenRepository.GetAll(new RefreshTokensByRelationSpec(userId)))
                    .Select(t => t.token_id));

                await notificationRepository.Add(new NotificationModel
                {
                    message_header = NotificationMessage.AUTH_TOKENS_REVOKED_HEADER,
                    message = NotificationMessage.AUTH_TOKENS_REVOKED_BODY,
                    is_checked = false,
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    user_id = userId!
                });

                await transaction.CommitAsync();
            }
            catch (EntityException)
            {
                throw;
            }
        }

        public bool IsValid(object ownRole, object? targetRole = null)
        {
            if (targetRole is null)
                return false;

            if (ownRole.Equals("HighestAdmin") && !targetRole.Equals("HighestAdmin"))
                return true;
            else
                return false;
        }
    }
}
