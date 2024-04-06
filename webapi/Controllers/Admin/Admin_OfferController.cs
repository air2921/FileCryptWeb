using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.DB;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/offers")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class Admin_OfferController(IRepository<OfferModel> offerRepository, IRedisCache redisCache, ISorting sorting) : ControllerBase
    {
        [HttpGet("{offerId}")]
        [ProducesResponseType(typeof(OfferModel), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetOffer([FromRoute] int offerId)
        {
            try
            {
                var offer = await offerRepository.GetById(offerId);
                if (offer is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                return StatusCode(200, new { offer });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("range")]
        [ProducesResponseType(typeof(IEnumerable<OfferModel>), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetRangeOffers([FromQuery] int? userId, [FromQuery] int skip, [FromQuery] int count,
            [FromQuery] bool byDesc, [FromQuery] bool? sended,
            [FromQuery] bool? isAccepted, [FromQuery] string? type)
        {
            try
            {
                return StatusCode(200, new { offers = await offerRepository
                    .GetAll(sorting.SortOffers(userId, skip, count, byDesc, sended, isAccepted, type)) });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{offerId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteOffer([FromRoute] int offerId)
        {
            try
            {
                var offer = await offerRepository.Delete(offerId);
                if (offer is not null)
                {
                    await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{offer.sender_id}");
                    await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{offer.receiver_id}");
                }

                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("range")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteRangeOffers([FromBody] IEnumerable<int> identifiers)
        {
            try
            {
                var offerList = await offerRepository.DeleteMany(identifiers);
                await redisCache.DeleteRedisCache(offerList, ImmutableData.OFFERS_PREFIX, item => item.sender_id);
                await redisCache.DeleteRedisCache(offerList, ImmutableData.OFFERS_PREFIX, item => item.receiver_id);

                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
