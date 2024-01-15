using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using webapi.DB;
using webapi.DB.SQL;
using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Localization.Exceptions;
using webapi.Models;

namespace webapi.Controllers.Core
{
    [Route("api/core/offers")]
    [ApiController]
    [Authorize]
    public class OfferController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IRedisCache _redisCache;
        private readonly IUserInfo _userInfo;
        private readonly ICreate<OfferModel> _createOffer;
        private readonly ICreate<NotificationModel> _createNotification;
        private readonly IRead<UserModel> _readUser;
        private readonly IRead<KeyModel> _readKeys;
        private readonly IRead<OfferModel> _readOffer;
        private readonly IDelete<OfferModel> _deleteOffer;
        private readonly ITokenService _tokenService;

        private const string OFFERS = "Cache_Offer_List";

        public OfferController(
            FileCryptDbContext dbContext,
            IRedisCache redisCache,
            IUserInfo userInfo,
            ICreate<OfferModel> createOffer,
            ICreate<NotificationModel> createNotification,
            IRead<UserModel> readUser,
            IRead<KeyModel> readKeys,
            IRead<OfferModel> readOffer,
            IDelete<OfferModel> deleteOffer,
            ITokenService tokenService)
        {
            _dbContext = dbContext;
            _redisCache = redisCache;
            _userInfo = userInfo;
            _createOffer = createOffer;
            _createNotification = createNotification;
            _readUser = readUser;
            _readKeys = readKeys;
            _readOffer = readOffer;
            _deleteOffer = deleteOffer;
            _tokenService = tokenService;
        }

        [HttpPost("new/{receiverId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOneOffer([FromRoute] int receiverId)
        {
            try
            {
                if (_userInfo.UserId == receiverId)
                    return StatusCode(409, new { message = "You want send a trade offer to yourself, are you kidding)?" });

                var receiver = await _readUser.ReadById(receiverId, null);

                var keys = await _readKeys.ReadById(_userInfo.UserId, true);
                if (keys is null)
                {
                    _tokenService.DeleteTokens();
                    return StatusCode(404);
                }

                if (keys.internal_key is null)
                    return StatusCode(404, new { message = "You don't have a internal key for create an offer" });

                var offerModel = new OfferModel
                {
                    offer_header = $"Proposal to accept an encryption key from a user: {_userInfo.Username}#{_userInfo.UserId}",
                    offer_body = keys.internal_key,
                    offer_type = TradeType.Key.ToString(),
                    is_accepted = false,
                    sender_id = _userInfo.UserId,
                    receiver_id = receiverId,
                    created_at = DateTime.UtcNow
                };

                var notificationModel = new NotificationModel
                {
                    message_header = "New offer",
                    message = $"You got a new offer from {_userInfo.Username}#{_userInfo.UserId}",
                    priority = Priority.Trade.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    receiver_id = receiverId
                };

                await _createOffer.Create(offerModel);
                await _createNotification.Create(notificationModel);
                HttpContext.Session.SetString(OFFERS, true.ToString());

                return StatusCode(201, new { message = SuccessMessage.SuccessOfferCreated });
            }
            catch (UserException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpPut("accept/{offerId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptOffer([FromRoute] int offerId)
        {
            var offer = await _dbContext.Offers.FirstOrDefaultAsync(o => o.offer_id == offerId && o.receiver_id == _userInfo.UserId);
            if (offer is null)
                return StatusCode(404);

            if (offer.is_accepted == true)
                return StatusCode(409, new { message = ExceptionOfferMessages.OfferIsAccepted });

            var receiver = await _dbContext.Keys.FirstOrDefaultAsync(u => u.user_id == _userInfo.UserId);
            if (receiver is null)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404);
            }

            receiver.received_key = offer.offer_body;
            offer.is_accepted = true;

            await _dbContext.SaveChangesAsync();
            HttpContext.Session.SetString(OFFERS, true.ToString());

            return StatusCode(200, new { message = SuccessMessage.SuccessOfferAccepted });
        }

        [HttpGet("{offerId}")]
        public async Task<IActionResult> GetOneOffer([FromRoute] int offerId)
        {
            var cacheKey = $"Offer_{offerId}";
            try
            {
                var cacheOffer = await _redisCache.GetCachedData(cacheKey);
                if (cacheOffer is not null)
                {
                    var cacheResult = JsonConvert.DeserializeObject<OfferModel>(cacheOffer);
                    if (cacheResult.sender_id != _userInfo.UserId || cacheResult.sender_id != _userInfo.UserId)
                        return StatusCode(404);

                    return StatusCode(200, new { offers = cacheResult, userId = _userInfo.UserId });
                }

                var offer = await _readOffer.ReadById(offerId, false);
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                await _redisCache.CacheData(cacheKey, JsonConvert.SerializeObject(offer, settings), TimeSpan.FromMinutes(10));

                if (offer.sender_id != _userInfo.UserId || offer.receiver_id != _userInfo.UserId)
                    return StatusCode(404);

                return StatusCode(200, new { offer, userId = _userInfo.UserId });
            }
            catch (OfferException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] int skip, [FromQuery] int count, [FromQuery] bool? sended = null, [FromQuery] bool? isAccepted = null)
        {
            var cacheKey = $"Offer_{_userInfo.UserId}_{skip}_{count}_{sended}_{isAccepted}";

            bool clearCache = HttpContext.Session.GetString(OFFERS) is not null ? bool.Parse(HttpContext.Session.GetString(OFFERS)) : true;

            if (clearCache)
            {
                await _redisCache.DeleteCache(cacheKey);
                HttpContext.Session.SetString(OFFERS, false.ToString());
            }

            var cacheOffers = await _redisCache.GetCachedData(cacheKey);
            if (cacheOffers is not null)
                return StatusCode(200, new { offers = JsonConvert.DeserializeObject<IEnumerable<OfferModel>>(cacheOffers), user_id = _userInfo.UserId });

            var query = _dbContext.Offers.OrderByDescending(o => o.created_at)
                .Select(o => new { o.offer_id, o.sender_id, o.receiver_id, o.created_at, o.is_accepted, o.offer_type })
                .AsQueryable();

            switch (sended)
            {
                case true:
                    query = query.Where(o => o.sender_id == _userInfo.UserId);
                    break;

                case false:
                    query = query.Where(o => o.receiver_id == _userInfo.UserId);
                    break;

                default:
                    query = query.Where(o => o.sender_id == _userInfo.UserId || o.receiver_id == _userInfo.UserId);
                    break;
            }

            switch (isAccepted)
            {
                case true:
                    query = query.Where(o => o.is_accepted == true);
                    break;

                case false:
                    query = query.Where(o => o.is_accepted == false);
                    break;

                default:
                    query = query.Where(o => o.is_accepted == true || o.is_accepted == false);
                    break;
            }

            var offers = await query
                .Skip(skip)
                .Take(count)
                .ToListAsync();

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            await _redisCache.CacheData(cacheKey, JsonConvert.SerializeObject(offers, settings), TimeSpan.FromMinutes(5));

            return StatusCode(200, new { offers, user_id = _userInfo.UserId });
        }

        [HttpDelete("{offerId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOneOffer([FromRoute] int offerId)
        {
            try
            {
                await _deleteOffer.DeleteById(offerId, _userInfo.UserId);
                HttpContext.Session.SetString(OFFERS, true.ToString());

                return StatusCode(200, new { message = SuccessMessage.SuccessOfferDeleted });
            }
            catch (OfferException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
