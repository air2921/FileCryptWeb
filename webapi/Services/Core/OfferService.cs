using webapi.DB.Abstractions;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Models;
using webapi.Services.Abstractions;

namespace webapi.Services.Core
{
    public class OfferService(
        IUserInfo userInfo,
        IRedisCache redisCache,
        IDatabaseTransaction transaction,
        IRepository<KeyModel> keyRepository,
        IRepository<OfferModel> offerRepository,
        IRepository<NotificationModel> notificationRepository) : IDataManagement, ITransaction<KeyModel>, ITransaction<Participants>
    {
        public async Task CreateTransaction(KeyModel keys, object? parameter = null)
        {
            try
            {
                var offer = parameter as OfferModel;

                keys.received_key = offer!.offer_body;
                offer.is_accepted = true;

                await keyRepository.Update(keys);
                await offerRepository.Update(offer);

                await transaction.CommitAsync();
            }
            catch (EntityNotUpdatedException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }

        public async Task CreateTransaction(Participants participants, object? parameter = null)
        {
            try
            {
                await offerRepository.Add(new OfferModel
                {
                    offer_header = $"Proposal to accept an encryption key from a user: {userInfo.Username}#{userInfo.UserId}",
                    offer_body = (string)parameter!,
                    offer_type = TradeType.Key.ToString(),
                    is_accepted = false,
                    sender_id = participants.SenderId,
                    receiver_id = participants.ReceiverId,
                    created_at = DateTime.UtcNow
                });

                await notificationRepository.Add(new NotificationModel
                {
                    message_header = "New offer",
                    message = $"You got a new offer from {userInfo.Username}#{userInfo.UserId}",
                    priority = Priority.Trade.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    user_id = participants.ReceiverId
                });

                await transaction.CommitAsync();
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

        public async Task DeleteData(int id, object? paramater = null)
        {
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{id}");
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{paramater}");
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{id}");
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{paramater}");
        }

        public Task<object> GetData(string key) => throw new NotImplementedException();

        public Task SetData(string key, object data) => throw new NotImplementedException();
    }

    public record Participants(int SenderId, int ReceiverId);
}
