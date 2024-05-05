using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;

namespace application.Helper_Services.Core
{
    public class KeyStorageItemHelper(
        IRepository<KeyStorageItemModel> storageItemRepository,
        IRepository<ActivityModel> activityRepository,
        IDatabaseTransaction dbTransaction) : ITransaction<KeyStorageItemModel>
    {
        public async Task CreateTransaction(KeyStorageItemModel data, object? parameter = null)
        {
            var strId = parameter as string ?? string.Empty;
            if (!int.TryParse(strId, out int id))
                throw new EntityException(Message.ERROR);

            using var transaction = await dbTransaction.BeginAsync();
            try
            {
                await storageItemRepository.Add(data);
                await activityRepository.Add(new ActivityModel
                {
                    user_id = id,
                    action_date = DateTime.UtcNow,
                    action_type = Activity.AddKey.ToString()
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
