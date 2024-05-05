using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;

namespace application.Helper_Services.Core
{
    public class KeyStorageHelper(
        IRepository<KeyStorageModel> storageRepository,
        IRepository<ActivityModel> activityRepository,
        IDatabaseTransaction dbTransaction) : ITransaction<KeyStorageModel>
    {
        public async Task CreateTransaction(KeyStorageModel data, object? parameter = null)
        {
            using var transaction = await dbTransaction.BeginAsync();
            try
            {
                await storageRepository.Add(data);
                await activityRepository.Add(new ActivityModel
                {
                    user_id = data.user_id,
                    action_date = DateTime.UtcNow,
                    action_type = Activity.AddStorage.ToString()
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
