﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DB;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/offers")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class Admin_OfferController : ControllerBase
    {
        private readonly IRepository<OfferModel> _offerRepository;
        private readonly IRedisCache _redisCache;
        private readonly ISorting _sorting;

        public Admin_OfferController(IRepository<OfferModel> offerRepository, IRedisCache redisCache, ISorting sorting)
        {
            _offerRepository = offerRepository;
            _redisCache = redisCache;
            _sorting = sorting;
        }

        [HttpGet("{offerId}")]
        public async Task<IActionResult> GetOffer([FromRoute] int offerId)
        {
            try
            {
                var offer = await _offerRepository.GetById(offerId);
                if (offer is null)
                    return StatusCode(404);

                return StatusCode(200, new { offer });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("many")]
        public async Task<IActionResult> GetRangeOffers([FromQuery] int? userId, [FromQuery] int skip, [FromQuery] int count,
            [FromQuery] bool byDesc, [FromQuery] bool? sended,
            [FromQuery] bool? isAccepted, [FromQuery] string? type)
        {
            try
            {
                return StatusCode(200, new { offers = await _offerRepository
                    .GetAll(_sorting.SortOffers(userId, skip, count, byDesc, sended, isAccepted, type)) });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{offerId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOffer([FromRoute] int offerId)
        {
            try
            {
                var offer = await _offerRepository.Delete(offerId);
                if (offer is not null)
                {
                    await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{offer.sender_id}");
                    await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{offer.receiver_id}");
                }

                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("many")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRangeOffers([FromBody] IEnumerable<int> identifiers)
        {
            try
            {
                var offerList = await _offerRepository.DeleteMany(identifiers);
                await _redisCache.DeleteRedisCache(offerList, ImmutableData.OFFERS_PREFIX, item => item.sender_id);
                await _redisCache.DeleteRedisCache(offerList, ImmutableData.OFFERS_PREFIX, item => item.receiver_id);

                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
