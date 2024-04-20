using application.DTO.Inner;
using application.Helpers;
using application.Services.Abstractions;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;

namespace application.Services.Additional.Core
{
    public class OfferHelper(
        IRepository<KeyStorageItemModel> storageItemRepository,
        IRepository<OfferModel> offerRepository,
        IRepository<NotificationModel> notificationRepository,
        IDatabaseTransaction transaction,
        IRedisCache redisCache) : ITransaction<CreateOfferDTO>, ITransaction<AcceptOfferDTO>, IDataManagement
    {
        public async Task CreateTransaction(CreateOfferDTO dto, object? parameter = null)
        {
            try
            {
                await offerRepository.Add(new OfferModel
                {
                    receiver_id = dto.ReceiverId,
                    sender_id = dto.SenderId,
                    offer_body = dto.KeyValue,
                    offer_header = $"Proposal to accept an encryption key from a user: #{dto.SenderId}",
                    created_at = DateTime.UtcNow,
                    is_accepted = false,
                    offer_type = TradeType.Key.ToString()
                });

                await notificationRepository.Add(new NotificationModel
                {
                    message_header = "New offer",
                    message = $"You got a new offer from #{dto.SenderId}",
                    priority = Priority.Trade.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    user_id = dto.ReceiverId
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

        public async Task CreateTransaction(AcceptOfferDTO dto, object? parameter = null)
        {
            try
            {
                await storageItemRepository.Add(new KeyStorageItemModel
                {
                    key_name = dto.KeyName,
                    storage_id = dto.StorageId,
                    created_at = DateTime.UtcNow,
                    key_value = dto.Offer.offer_body
                });

                dto.Offer.is_accepted = true;
                await offerRepository.Update(dto.Offer);

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

        public async Task DeleteData(int id, object? parameter = null)
        {
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{id}");
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{parameter}");
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{id}");
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{parameter}");
        }

        public Task<object> GetData(string key)
        {
            throw new NotImplementedException();
        }

        public Task SetData(string key, object data)
        {
            throw new NotImplementedException();
        }
    }
}
