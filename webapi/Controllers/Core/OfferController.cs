using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.DB.Abstractions;
using webapi.DB.Ef.Specifications.By_Relation_Specifications;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Abstractions;
using webapi.Services.Core;
using webapi.Services.Core.Data_Handlers;

namespace webapi.Controllers.Core
{
    [Route("api/core/offers")]
    [ApiController]
    [Authorize]
    [EntityExceptionFilter]
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
            if (userInfo.UserId.Equals(receiverId))
                return StatusCode(409, new { message = Message.CONFLICT });

            var receiver = await userRepository.GetById(receiverId);
            if (receiver is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            var keys = await keyRepository.GetByFilter(new KeysByRelationSpec(userInfo.UserId));
            if (keys is null || keys.internal_key is null)
                return StatusCode(404, new { message = "You don't have a internal key for create an offer" });

            await participantsTransaction.CreateTransaction(new Participants(userInfo.UserId, receiverId), keys.internal_key);
            await dataManagement.DeleteData(userInfo.UserId, receiverId);

            return StatusCode(201, new { message = Message.CREATED });
        }

        [HttpPut("accept/{offerId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 409)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> AcceptOffer([FromRoute] int offerId)
        {
            var offer = await offerRepository.GetByFilter(new OfferByIdAndRelationSpec(offerId, userInfo.UserId, false));
            if (offer is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            if (offer.is_accepted)
                return StatusCode(409, new { message = Message.CONFLICT });

            var receiver = await keyRepository.GetByFilter(new KeysByRelationSpec(offer.receiver_id));
            if (receiver is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            await keyTransaction.CreateTransaction(receiver, offer);
            await dataManagement.DeleteData(offer.sender_id, userInfo.UserId);

            return StatusCode(200, new { message = Message.UPDATED });
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
            var offer = await offerRepository.DeleteByFilter(new OfferByIdAndRelationSpec(offerId, userInfo.UserId, null));

            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{offer.sender_id}");
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{offer.receiver_id}");

            return StatusCode(204);
        }
    }
}
