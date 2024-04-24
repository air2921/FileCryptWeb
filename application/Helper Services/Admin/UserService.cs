using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using domain.Specifications.By_Relation_Specifications;

namespace application.Helper_Services.Admin
{
    public class UserService(
        IRepository<UserModel> userRepository,
        IRepository<TokenModel> tokenRepository) : ITransaction<UserModel>, IValidator
    {
        public async Task CreateTransaction(UserModel target, object? parameter = null)
        {
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
            }
            catch (EntityException)
            {
                throw;
            }
        }

        public bool IsValid(object role, object? parameter = null) => !role.Equals("HighestAdmin");
    }
}
