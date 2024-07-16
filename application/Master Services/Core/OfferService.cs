using application.DTO.Inner;
using application.Helpers;
using application.Helpers.Localization;
using application.Cache_Handlers;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using domain.Specifications.By_Relation_Specifications;
using Microsoft.Extensions.DependencyInjection;
using application.Helper_Services;
using application.Abstractions.TP_Services;

namespace application.Master_Services.Core
{
    public class OfferService(
        IRepository<UserModel> userRepository,
        IRepository<OfferModel> offerRepository,
        IRepository<KeyStorageModel> storageRepository,
        ITransaction<AcceptOfferDTO> acceptTransaction,
        ITransaction<CreateOfferDTO> createTransaction,
        ICacheHandler<KeyStorageItemModel> itemCacheHandler,
        ICacheHandler<OfferModel> offerCacheHandler,
        [FromKeyedServices(ImplementationKey.CORE_OFFER_SERVICE)] IDataManagement data,
        IHashUtility hashUtility)
    {
        public async Task<Response> Add(int senderId, int receiverId, int keyId, int storageId, string code)
        {
            try
            {
                if (senderId.Equals(receiverId))
                    return new Response { Status = 409, Message = Message.CONFLICT };

                var receiver = await userRepository.GetById(receiverId);
                if (receiver is null)
                    return new Response { Status = 404, Message = "Receiver not found" };

                var cacheKey = $"{ImmutableData.STORAGE_ITEMS_PREFIX}{senderId}_{keyId}_{storageId}";
                var key = await itemCacheHandler.CacheAndGet(
                    new StorageItemObject(cacheKey, senderId, keyId, storageId, code));

                if (key is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                await createTransaction.CreateTransaction(new CreateOfferDTO
                {
                    KeyValue = key.key_value,
                    SenderId = senderId,
                    ReceiverId = receiverId,
                });

                await data.DeleteData(senderId, receiverId);

                return new Response { Status = 201, Message = Message.CREATED };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (FormatException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> Accept(int userId, string keyName, int offerId, int storageId, string code)
        {
            try
            {
                var storage = await storageRepository.GetByFilter(new StorageByIdAndRelationSpec(storageId, userId));
                if (storage is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                if (!hashUtility.Verify(code, storage.access_code))
                    return new Response { Status = 403, Message = Message.INCORRECT };

                var offer = await offerRepository.GetById(offerId);
                if (offer is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                if (offer.is_accepted)
                    return new Response { Status = 409, Message = "Offer is already accepted" };

                if (offer.sender_id.Equals(userId))
                    return new Response { Status = 409, Message = Message.CONFLICT };

                await acceptTransaction.CreateTransaction(new AcceptOfferDTO
                {
                    KeyName = keyName,
                    StorageId = storageId,
                    Offer = offer
                });

                await data.DeleteData(offer.sender_id, offer.receiver_id);

                return new Response { Status = 200, Message = Message.UPDATED };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> GetOne(int userId, int offerId, bool bodyHide)
        {
            try
            {
                var cacheKey = $"{ImmutableData.OFFERS_PREFIX}{userId}_{offerId}";
                var offer = await offerCacheHandler.CacheAndGet(new OfferObject(cacheKey, userId, offerId));
                if (offer is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                offer.offer_body = bodyHide ? "hidden" : offer.offer_body;
                return new Response { Status = 200, ObjectData = offer };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (FormatException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> GetRange(int userId, int skip, int count, bool byDesc,
            bool? sended, bool? isAccepted, int? type, bool bodyHide)
        {
            try
            {
                var cacheKey = $"{ImmutableData.OFFERS_PREFIX}{userId}_{skip}_{count}_{byDesc}_{sended}_{isAccepted}_{type}";
                var obj = new OfferRangeObject(cacheKey, userId, skip, count, byDesc, sended, isAccepted, type);
                var offers = await offerCacheHandler.CacheAndGetRange(obj);

                if (bodyHide)
                    foreach (var offer in offers)
                        offer.offer_body = "hidden";

                return new Response
                {
                    Status = 200,
                    ObjectData = offers
                };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (FormatException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> DeleteOne(int userId, int offerId)
        {
            try
            {
                var offer = await offerRepository.DeleteByFilter(new OfferByIdAndRelationSpec(offerId, userId));
                if (offer is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                await data.DeleteData(offer.sender_id, offer.receiver_id);
                return new Response { Status = 204 };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }
    }
}
