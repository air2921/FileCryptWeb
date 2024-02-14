using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Localization.Exceptions;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers.Core
{
    [Route("api/core/offers")]
    [ApiController]
    [Authorize]
    public class OfferController : ControllerBase
    {
        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<OfferModel> _offerRepository;
        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly IRepository<KeyModel> _keyRepository;
        private readonly ISorting _sorting;
        private readonly IRedisCache _redisCache;
        private readonly IUserInfo _userInfo;

        public OfferController(
            IRepository<UserModel> userRepository,
            IRepository<OfferModel> offerRepository,
            IRepository<NotificationModel> notificationRepository,
            IRepository<KeyModel> keyRepository,
            ISorting sorting,
            IRedisCache redisCache,
            IUserInfo userInfo)
        {
            _userRepository = userRepository;
            _offerRepository = offerRepository;
            _notificationRepository = notificationRepository;
            _keyRepository = keyRepository;
            _sorting = sorting;
            _redisCache = redisCache;
            _userInfo = userInfo;
        }

        [HttpPost("new/{receiverId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOneOffer([FromRoute] int receiverId)
        {
            try
            {
                if (_userInfo.UserId.Equals(receiverId))
                    return StatusCode(409, new { message = "You want send a trade offer to yourself, are you kidding)?" });

                var receiver = await _userRepository.GetById(receiverId);
                if (receiver is null)
                    return StatusCode(404);

                var keys = await _keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(_userInfo.UserId)));
                if (keys is null)
                    return StatusCode(404);

                if (keys.internal_key is null)
                    return StatusCode(404, new { message = "You don't have a internal key for create an offer" });

                await _offerRepository.Add(new OfferModel
                {
                    offer_header = $"Proposal to accept an encryption key from a user: {_userInfo.Username}#{_userInfo.UserId}",
                    offer_body = keys.internal_key,
                    offer_type = TradeType.Key.ToString(),
                    is_accepted = false,
                    sender_id = _userInfo.UserId,
                    receiver_id = receiverId,
                    created_at = DateTime.UtcNow
                });

                await _notificationRepository.Add(new NotificationModel
                {
                    message_header = "New offer",
                    message = $"You got a new offer from {_userInfo.Username}#{_userInfo.UserId}",
                    priority = Priority.Trade.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    receiver_id = receiverId
                });

                await _redisCache.DeteteCacheByKeyPattern($"Notifications_{receiverId}");
                await _redisCache.DeteteCacheByKeyPattern($"Offers_{receiverId}");
                await _redisCache.DeteteCacheByKeyPattern($"Offers_{_userInfo.UserId}");

                return StatusCode(201, new { message = SuccessMessage.SuccessOfferCreated });
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("accept/{offerId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptOffer([FromRoute] int offerId)
        {
            try
            {
                var offer = await _offerRepository.GetByFilter(query => query.Where(o => o.offer_id.Equals(offerId) && o.receiver_id.Equals(_userInfo.UserId)));
                if (offer is null)
                    return StatusCode(404);

                if (offer.is_accepted)
                    return StatusCode(409, new { message = ExceptionOfferMessages.OfferIsAccepted });

                var receiver = await _keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(offer.receiver_id)));
                if (receiver is null)
                    return StatusCode(404);

                receiver.received_key = offer.offer_body;
                offer.is_accepted = true;

                await _keyRepository.Update(receiver);
                await _offerRepository.Update(offer);

                await _redisCache.DeteteCacheByKeyPattern($"Offers_{offer.sender_id}");
                await _redisCache.DeteteCacheByKeyPattern($"Offers_{_userInfo.UserId}");

                return StatusCode(200, new { message = SuccessMessage.SuccessOfferAccepted });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{offerId}")]
        public async Task<IActionResult> GetOneOffer([FromRoute] int offerId)
        {
            var cacheKey = $"Offers_{offerId}";

            var cacheOffer = await _redisCache.GetCachedData(cacheKey);
            if (cacheOffer is not null)
            {
                var cacheResult = JsonConvert.DeserializeObject<OfferModel>(cacheOffer);
                if (cacheResult.sender_id != _userInfo.UserId || cacheResult.sender_id != _userInfo.UserId)
                    return StatusCode(404);

                return StatusCode(200, new { offers = cacheResult, userId = _userInfo.UserId });
            }

            var offer = await _offerRepository.GetById(offerId);
            if (offer is null || offer.sender_id != _userInfo.UserId || offer.receiver_id != _userInfo.UserId)
                return StatusCode(404);

            await _redisCache.CacheData(cacheKey, offer, TimeSpan.FromMinutes(10));

            return StatusCode(200, new { offer, userId = _userInfo.UserId });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] int skip, [FromQuery] int count,
            [FromQuery] bool byDesc, [FromQuery] bool? sended,
            [FromQuery] bool? isAccepted, [FromQuery] string? type)
        {
            var cacheKey = $"Offers_{_userInfo.UserId}_{skip}_{count}_{byDesc}_{sended}_{isAccepted}_{type}";

            var cacheOffers = await _redisCache.GetCachedData(cacheKey);
            if (cacheOffers is not null)
                return StatusCode(200, new { offers = JsonConvert.DeserializeObject<IEnumerable<OfferModel>>(cacheOffers), user_id = _userInfo.UserId });

            var offers = await _offerRepository.GetAll(_sorting.SortOffers(_userInfo.UserId, skip, count, byDesc, sended, isAccepted, type));
            foreach (var offer in offers)
            {
                offer.offer_body = string.Empty;
                offer.offer_header = string.Empty;
            }

            await _redisCache.CacheData(cacheKey, offers, TimeSpan.FromMinutes(3));

            return StatusCode(200, new { offers, user_id = _userInfo.UserId });
        }

        [HttpDelete("{offerId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOneOffer([FromRoute] int offerId)
        {
            try
            {
                await _offerRepository.DeleteByFilter(query => query.Where
                (o => o.offer_id.Equals(offerId) && (o.sender_id.Equals(_userInfo.UserId) || o.receiver_id.Equals(_userInfo.UserId))));

                await _redisCache.DeteteCacheByKeyPattern($"Offers_{_userInfo.UserId}");

                return StatusCode(200, new { message = SuccessMessage.SuccessOfferDeleted });
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
