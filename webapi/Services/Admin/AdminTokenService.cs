using webapi.DB.Abstractions;
using webapi.DB.Ef.Specifications.By_Relation_Specifications;
using webapi.Exceptions;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Abstractions;

namespace webapi.Services.Admin
{
    public class AdminTokenService(
        IDatabaseTransaction transaction,
        IRepository<TokenModel> tokenRepository,
        IRepository<NotificationModel> notificationRepository) : ITransaction<TokenModel>, IValidator
    {
        public async Task CreateTransaction(TokenModel data, object? parameter = null)
        {
            try
            {
                if (!int.TryParse(parameter?.ToString(), out int userId))
                    throw new EntityNotDeletedException("Error when deleting data");

                var tokenIdentifiers = (await tokenRepository.GetAll(new RefreshTokensByRelationSpec(userId)))
                    .Select(t => t.token_id);

                await tokenRepository.DeleteMany(tokenIdentifiers);
                await notificationRepository.Add(new NotificationModel
                {
                    message_header = NotificationMessage.AUTH_TOKENS_REVOKED_HEADER,
                    message = NotificationMessage.AUTH_TOKENS_REVOKED_BODY,
                    is_checked = false,
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    user_id = (int)parameter!
                });

                await transaction.CommitAsync();
            }
            catch (OperationCanceledException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (EntityNotDeletedException)
            {
                await transaction.RollbackAsync();
                throw;
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

        public bool IsValid(object targetRole, object ownRole = null) => ownRole.Equals("HighestAdmin") && !targetRole.Equals("HighestAdmin");
    }
}
