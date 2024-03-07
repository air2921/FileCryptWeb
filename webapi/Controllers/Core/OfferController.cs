using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using webapi.Attributes;
using webapi.DB;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Core
{
    [Route("api/core/offers")]
    [ApiController]
    [Authorize]
    public class OfferController : ControllerBase
    {
        #region fields and constructor

        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<OfferModel> _offerRepository;
        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly IRepository<KeyModel> _keyRepository;
        private readonly FileCryptDbContext _dbContext;
        private readonly ISorting _sorting;
        private readonly IRedisCache _redisCache;
        private readonly IUserInfo _userInfo;

        public OfferController(
            IRepository<UserModel> userRepository,
            IRepository<OfferModel> offerRepository,
            IRepository<NotificationModel> notificationRepository,
            IRepository<KeyModel> keyRepository,
            FileCryptDbContext dbContext,
            ISorting sorting,
            IRedisCache redisCache,
            IUserInfo userInfo)
        {
            _userRepository = userRepository;
            _offerRepository = offerRepository;
            _notificationRepository = notificationRepository;
            _keyRepository = keyRepository;
            _dbContext = dbContext;
            _sorting = sorting;
            _redisCache = redisCache;
            _userInfo = userInfo;
        }

        #endregion

        [HttpPost("new/{receiverId}")]
        [XSRFProtection]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 409)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> CreateOneOffer([FromRoute] int receiverId)
        {
            try
            {
                if (_userInfo.UserId.Equals(receiverId))
                    return StatusCode(409, new { message = Message.CONFLICT });

                var receiver = await _userRepository.GetById(receiverId);
                if (receiver is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                var keys = await _keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(_userInfo.UserId)));
                if (keys is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (keys.internal_key is null)
                    return StatusCode(404, new { message = "You don't have a internal key for create an offer" });

                await CreateOfferTransaction(_userInfo.UserId, receiverId, keys.internal_key);
                await ClearData(_userInfo.UserId, receiverId);

                return StatusCode(201, new { message = Message.CREATED });
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("accept/{offerId}")]
        [XSRFProtection]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 409)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> AcceptOffer([FromRoute] int offerId)
        {
            try
            {
                var offer = await _offerRepository.GetByFilter(query => query.Where(o => o.offer_id.Equals(offerId) && o.receiver_id.Equals(_userInfo.UserId)));
                if (offer is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (offer.is_accepted)
                    return StatusCode(409, new { message = Message.CONFLICT });

                var receiver = await _keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(offer.receiver_id)));
                if (receiver is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                await AcceptOfferTransaction(receiver, offer);
                await ClearData(offer.sender_id, _userInfo.UserId);

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
                var cacheKey = $"{ImmutableData.OFFERS_PREFIX}{_userInfo.UserId}_{offerId}";

                var cacheOffer = await _redisCache.GetCachedData(cacheKey);
                if (cacheOffer is not null)
                    return StatusCode(200, new { offers = JsonConvert.DeserializeObject<OfferModel>(cacheOffer) });

                var offer = await _offerRepository.GetById(offerId);
                if (offer is null || offer.sender_id != _userInfo.UserId || offer.receiver_id != _userInfo.UserId)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                await _redisCache.CacheData(cacheKey, offer, TimeSpan.FromMinutes(10));

                return StatusCode(200, new { offer, userId = _userInfo.UserId });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
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
                var cacheKey = $"{ImmutableData.OFFERS_PREFIX}{_userInfo.UserId}_{skip}_{count}_{byDesc}_{sended}_{isAccepted}_{type}";

                var cacheOffers = await _redisCache.GetCachedData(cacheKey);
                if (cacheOffers is not null)
                    return StatusCode(200, new { offers = JsonConvert.DeserializeObject<IEnumerable<OfferModel>>(cacheOffers), user_id = _userInfo.UserId });

                var offers = await _offerRepository.GetAll(_sorting
                    .SortOffers(_userInfo.UserId, skip, count, byDesc, sended, isAccepted, type));
                foreach (var offer in offers)
                {
                    offer.offer_body = string.Empty;
                    offer.offer_header = string.Empty;
                }

                await _redisCache.CacheData(cacheKey, offers, TimeSpan.FromMinutes(3));

                return StatusCode(200, new { offers, user_id = _userInfo.UserId });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{offerId}")]
        [XSRFProtection]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> DeleteOneOffer([FromRoute] int offerId)
        {
            try
            {
                var offer = await _offerRepository.DeleteByFilter(query => query
                .Where(o => o.offer_id.Equals(offerId) && (o.sender_id.Equals(_userInfo.UserId) || o.receiver_id.Equals(_userInfo.UserId))));

                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{offer.sender_id}");
                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{offer.receiver_id}");

                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [Helper]
        private async Task CreateOfferTransaction(int senderId, int receiverId, string internalKey)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await _offerRepository.Add(new OfferModel
                {
                    offer_header = $"Proposal to accept an encryption key from a user: {_userInfo.Username}#{_userInfo.UserId}",
                    offer_body = internalKey,
                    offer_type = TradeType.Key.ToString(),
                    is_accepted = false,
                    sender_id = senderId,
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
                    user_id = receiverId
                });

                await transaction.CommitAsync();
            }
            catch (EntityNotCreatedException)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [Helper]
        private async Task AcceptOfferTransaction(KeyModel receiverKeys, OfferModel offer)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                receiverKeys.received_key = offer.offer_body;
                offer.is_accepted = true;

                await _keyRepository.Update(receiverKeys);
                await _offerRepository.Update(offer);

                await transaction.CommitAsync();
            }
            catch (EntityNotUpdatedException)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [Helper]
        private async Task ClearData(int senderId, int receiverId)
        {
            await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{receiverId}");
            await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{senderId}");
            await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{receiverId}");
            await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{senderId}");
        }
    }
}
