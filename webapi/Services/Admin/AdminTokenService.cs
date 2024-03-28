using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Localization;
using webapi.Models;

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
                var tokenIdentifiers = (await tokenRepository.GetAll(query => query.Where(t => t.user_id.Equals((int)parameter!))))
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
