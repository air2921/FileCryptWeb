using Newtonsoft.Json;
using webapi.DB.Abstractions;
using webapi.DB.Ef;
using webapi.Localization;
using webapi.Models;

namespace webapi.Services.Core.Data_Handlers
{
    public class Offers(
        IRepository<OfferModel> offerRepository,
        ISorting sorting,
        IRedisCache redisCache,
        ILogger<Offers> logger) : ICacheHandler<OfferModel>
    {
        public async Task<OfferModel> CacheAndGet(object dataObject)
        {
            try
            {
                var offerObj = dataObject as OfferObject ?? throw new FormatException(Message.ERROR);
                var offer = new OfferModel();
                var cache = await redisCache.GetCachedData(offerObj.CacheKey);
                if (cache is null)
                {
                    offer = await offerRepository.GetByFilter(query => query
                        .Where(o => o.offer_id.Equals(offerObj.OfferId) &&
                        (o.sender_id.Equals(offerObj.UserId) || o.receiver_id.Equals(offerObj.OfferId))));

                    if (offer is null)
                        return null;

                    offer.offer_header = string.Empty;
                    offer.offer_body = string.Empty;

                    await redisCache.CacheData(offerObj.CacheKey, offer, TimeSpan.FromMinutes(10));
                    return offer;
                }

                offer = JsonConvert.DeserializeObject<OfferModel>(cache);
                if (offer is not null)
                    return offer;
                else
                    return null;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(Files));
                throw new FormatException(Message.ERROR);
            }
        }

        public async Task<IEnumerable<OfferModel>> CacheAndGetRange(object dataObject)
        {
            try
            {
                var offerObj = dataObject as OfferRangeObject ?? throw new FormatException(Message.ERROR);
                var offers = new List<OfferModel>();
                var cache = await redisCache.GetCachedData(offerObj.CacheKey);
                if (cache is null)
                {
                    offers = (List<OfferModel>)await offerRepository.GetAll(sorting
                        .SortOffers(offerObj.UserId, offerObj.Skip, offerObj.Count, offerObj.ByDesc, offerObj.Sended, offerObj.IsAccepted, offerObj.Type));

                    foreach (var offer in offers)
                    {
                        offer.offer_header = string.Empty;
                        offer.offer_body = string.Empty;
                    }

                    await redisCache.CacheData(offerObj.CacheKey, offers, TimeSpan.FromMinutes(10));
                    return offers;
                }

                offers = JsonConvert.DeserializeObject<List<OfferModel>>(cache);
                if (offers is not null)
                    return offers;
                else
                    throw new FormatException(Message.ERROR);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(Offers));
                throw new FormatException(Message.ERROR);
            }
        }
    }

    public record class OfferObject(string CacheKey, int UserId, int OfferId);
    public record class OfferRangeObject(string CacheKey, int UserId, int Skip, int Count, bool ByDesc, bool? Sended, bool? IsAccepted, string? Type);
}
