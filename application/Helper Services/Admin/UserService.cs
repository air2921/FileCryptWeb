using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using domain.Specifications.By_Relation_Specifications;

namespace application.Helper_Services.Admin
{
    public class UserService(
        IRepository<UserModel> userRepository,
        IRepository<TokenModel> tokenRepository,
        IDatabaseTransaction dbTransaction) : ITransaction<UserModel>, IValidator
    {
        public async Task CreateTransaction(UserModel target, object? parameter = null)
        {
            using var transaction = await dbTransaction.BeginAsync();
            try
            {
                if (!bool.TryParse(parameter?.ToString(), out bool block))
                    throw new EntityException("Error when updating data");

                target.is_blocked = block;
                await userRepository.Update(target);

                if (block)
                    await tokenRepository.DeleteMany((await tokenRepository
                        .GetAll(new RefreshTokensByRelationSpec(target.id)))
                        .Select(t => t.token_id));

                await dbTransaction.CommitAsync(transaction);
            }
            catch (EntityException)
            {
                await dbTransaction.RollbackAsync(transaction);
                throw;
            }
        }

        public bool IsValid(object role, object? parameter = null) => !role.Equals("HighestAdmin");
    }
}
