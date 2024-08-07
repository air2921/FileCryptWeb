﻿using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using domain.Specifications.By_Relation_Specifications;
using domain.Specifications.Sorting_Specifications;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace application.Cache_Handlers
{
    public class Offers(
        IRepository<OfferModel> offerRepository,
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
                    offer = await offerRepository.GetByFilter(
                        new OfferByIdAndRelationSpec(offerObj.OfferId, offerObj.UserId));

                    if (offer is null)
                        return null;

                    await redisCache.CacheData(offerObj.CacheKey, offer, TimeSpan.FromMinutes(10));
                    return offer;
                }

                offer = JsonConvert.DeserializeObject<OfferModel>(cache);
                if (offer is not null)
                    return offer;
                else
                    return null;
            }
            catch (EntityException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(Offers));
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
                    offers = (List<OfferModel>)await offerRepository.GetAll(
                        new OffersSortSpec(offerObj.UserId, offerObj.Skip, offerObj.Count, offerObj.ByDesc, offerObj.Sended, offerObj.IsAccepted, offerObj.Type));

                    await redisCache.CacheData(offerObj.CacheKey, offers, TimeSpan.FromMinutes(10));
                    return offers;
                }

                offers = JsonConvert.DeserializeObject<List<OfferModel>>(cache);
                if (offers is not null)
                    return offers;
                else
                    throw new FormatException(Message.ERROR);
            }
            catch (EntityException)
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
    public record class OfferRangeObject(string CacheKey, int UserId, int Skip, int Count, bool ByDesc, bool? Sended, bool? IsAccepted, int? Type);
}
