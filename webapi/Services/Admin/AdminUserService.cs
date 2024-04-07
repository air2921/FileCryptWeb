using webapi.DB.Abstractions;
using webapi.DB.Ef.Specifications.By_Relation_Specifications;
using webapi.Exceptions;
using webapi.Models;
using webapi.Services.Abstractions;

namespace webapi.Services.Admin
{
    public class AdminUserService(
        IDatabaseTransaction transaction,
        IRepository<UserModel> userRepository,
        IRepository<TokenModel> tokenRepository) : ITransaction<UserModel>, IValidator
    {
        public async Task CreateTransaction(UserModel target, object? parameter = null)
        {
            try
            {
                if (!bool.TryParse(parameter?.ToString(), out bool block))
                    throw new EntityNotUpdatedException("Error when updating data");

                target.is_blocked = block;
                await userRepository.Update(target);

                if (block)
                {
                    var tokenIdentifiers = (await tokenRepository.GetAll(new RefreshTokensByRelationSpec(target.id)))
                        .Select(t => t.token_id);
                    await tokenRepository.DeleteMany(tokenIdentifiers);
                }

                await transaction.CommitAsync();
            }
            catch (EntityNotUpdatedException)
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
                throw new EntityNotUpdatedException("Error when updating data");
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }

        public bool IsValid(object role, object parameter = null) => !role.Equals("HighestAdmin");
    }
}
