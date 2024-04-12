using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.DB.Abstractions;
using webapi.DB.Ef.Specifications.Sorting_Specifications;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/offers")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    [EntityExceptionFilter]
    public class Admin_OfferController(IRepository<OfferModel> offerRepository, IRedisCache redisCache) : ControllerBase
    {
        [HttpGet("{offerId}")]
        [ProducesResponseType(typeof(OfferModel), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetOffer([FromRoute] int offerId)
        {
            var offer = await offerRepository.GetById(offerId);
            if (offer is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            return StatusCode(200, new { offer });
        }

        [HttpGet("range")]
        [ProducesResponseType(typeof(IEnumerable<OfferModel>), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetRangeOffers([FromQuery] int? userId, [FromQuery] int skip, [FromQuery] int count,
            [FromQuery] bool byDesc, [FromQuery] bool? sended,
            [FromQuery] bool? isAccepted, [FromQuery] string? type)
        {
            return StatusCode(200, new { offers = await offerRepository
                .GetAll(new OffersSortSpec(userId, skip, count, byDesc, sended, isAccepted, type))});
        }

        [HttpDelete("{offerId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteOffer([FromRoute] int offerId)
        {
            var offer = await offerRepository.Delete(offerId);
            if (offer is not null)
            {
                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{offer.sender_id}");
                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{offer.receiver_id}");
            }

            return StatusCode(204);
        }

        [HttpDelete("range")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteRangeOffers([FromBody] IEnumerable<int> identifiers)
        {
            var offerList = await offerRepository.DeleteMany(identifiers);
            await redisCache.DeleteRedisCache(offerList, ImmutableData.OFFERS_PREFIX, item => item.sender_id);
            await redisCache.DeleteRedisCache(offerList, ImmutableData.OFFERS_PREFIX, item => item.receiver_id);

            return StatusCode(204);
        }
    }
}
