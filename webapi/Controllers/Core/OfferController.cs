﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Core;
using webapi.Services.Core.Data_Handlers;

namespace webapi.Controllers.Core
{
    [Route("api/core/offers")]
    [ApiController]
    [Authorize]
    public class OfferController(
        [FromKeyedServices(ImplementationKey.CORE_OFFER_SERVICE)] IDataManagement dataManagement,
        [FromKeyedServices(ImplementationKey.CORE_OFFER_SERVICE)] ITransaction<KeyModel> keyTransaction,
        [FromKeyedServices(ImplementationKey.CORE_OFFER_SERVICE)] ITransaction<Participants> participantsTransaction,
        IRepository<UserModel> userRepository,
        IRepository<OfferModel> offerRepository,
        IRepository<KeyModel> keyRepository,
        ICacheHandler<OfferModel> cache,
        IRedisCache redisCache,
        IUserInfo userInfo) : ControllerBase
    {
        [HttpPost("new/{receiverId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 409)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> CreateOneOffer([FromRoute] int receiverId)
        {
            try
            {
                if (userInfo.UserId.Equals(receiverId))
                    return StatusCode(409, new { message = Message.CONFLICT });

                var receiver = await userRepository.GetById(receiverId);
                if (receiver is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                var keys = await keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(userInfo.UserId)));
                if (keys is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (keys.internal_key is null)
                    return StatusCode(404, new { message = "You don't have a internal key for create an offer" });

                await participantsTransaction.CreateTransaction(new Participants(userInfo.UserId, receiverId), keys.internal_key);
                await dataManagement.DeleteData(userInfo.UserId, receiverId);

                return StatusCode(201, new { message = Message.CREATED });
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("accept/{offerId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 409)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> AcceptOffer([FromRoute] int offerId)
        {
            try
            {
                var offer = await offerRepository.GetByFilter(query => query.Where(o => o.offer_id.Equals(offerId) && o.receiver_id.Equals(userInfo.UserId)));
                if (offer is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (offer.is_accepted)
                    return StatusCode(409, new { message = Message.CONFLICT });

                var receiver = await keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(offer.receiver_id)));
                if (receiver is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                await keyTransaction.CreateTransaction(receiver, offer);
                await dataManagement.DeleteData(offer.sender_id, userInfo.UserId);

                return StatusCode(200, new { message = Message.UPDATED });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{offerId}")]
        [ProducesResponseType(typeof(OfferModel), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetOneOffer([FromRoute] int offerId)
        {
            try
            {
                var cacheKey = $"{ImmutableData.OFFERS_PREFIX}{userInfo.UserId}_{offerId}";
                var offer = await cache.CacheAndGet(new OfferObject(cacheKey, userInfo.UserId, offerId));
                if (offer is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                return StatusCode(200, new { offer, userId = userInfo.UserId });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (FormatException)
            {
                return StatusCode(500, new { message = Message.ERROR });
            }
        }

        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<OfferModel>), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetAll([FromQuery] int skip, [FromQuery] int count,
            [FromQuery] bool byDesc, [FromQuery] bool? sended,
            [FromQuery] bool? isAccepted, [FromQuery] string? type)
        {
            try
            {
                var cacheKey = $"{ImmutableData.OFFERS_PREFIX}{userInfo.UserId}_{skip}_{count}_{byDesc}_{sended}_{isAccepted}_{type}";
                var offers = await cache.CacheAndGetRange(new OfferRangeObject(cacheKey, userInfo.UserId, skip, count, byDesc, sended, isAccepted, type));

                return StatusCode(200, new { offers, user_id = userInfo.UserId });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (FormatException)
            {
                return StatusCode(500, new { message = Message.ERROR });
            }
        }

        [HttpDelete("{offerId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> DeleteOneOffer([FromRoute] int offerId)
        {
            try
            {
                var offer = await offerRepository.DeleteByFilter(query => query
                    .Where(o => o.offer_id.Equals(offerId) && (o.sender_id.Equals(userInfo.UserId) || o.receiver_id.Equals(userInfo.UserId))));

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{offer.sender_id}");
                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{offer.receiver_id}");

                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
