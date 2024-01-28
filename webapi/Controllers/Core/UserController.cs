using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization.Exceptions;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers.Core
{
    [Route("api/core/users")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IRedisCache _redisCache;
        private readonly IDelete<UserModel> _deleteUser;
        private readonly IUserInfo _userInfo;
        private readonly ITokenService _tokenService;
        private readonly IRead<UserModel> _readUser;
        private readonly IRead<FileModel> _readFile;
        private readonly IRead<OfferModel> _readOffer;

        public UserController(
            FileCryptDbContext dbContext,
            IRedisCache redisCache,
            IUserInfo userInfo,
            ITokenService tokenService,
            IRead<UserModel> readUser,
            IRead<FileModel> readFile,
            IRead<OfferModel> readOffer,
            IDelete<UserModel> deleteUser)
        {
            _dbContext = dbContext;
            _redisCache = redisCache;
            _userInfo = userInfo;
            _tokenService = tokenService;
            _readUser = readUser;
            _readFile = readFile;
            _readOffer = readOffer;
            _deleteUser = deleteUser;
        }

        [HttpGet("{userId}/{username}")]
        public async Task<IActionResult> GetUser([FromRoute] int userId, [FromRoute] string username)
        {
            var userAndKeys = await _dbContext.Users
                    .Where(u => u.id == userId && u.username == username)
                    .Join(_dbContext.Keys, user => user.id, keys => keys.user_id, (user, keys) => new { user, keys })
                    .FirstOrDefaultAsync();

            if (userAndKeys is null)
                return StatusCode(404, new { message = ExceptionUserMessages.UserNotFound });

            var cacheKeyFiles = $"Profile_Files_{userId}";
            var cacheKeyOffers = $"Profile_Offers_{userId}";
            var list_offers = new List<OfferObject>();
            var files = new List<FileModel>();
            var offers = new List<OfferModel>();

            var cacheFiles = await _redisCache.GetCachedData(cacheKeyFiles);
            if (cacheFiles is not null)
            {
                files = JsonConvert.DeserializeObject<List<FileModel>>(cacheFiles);
            }
            else
            {
                var filesDb = await _readFile.ReadAll(userId, 0, 5);
                await _redisCache.CacheData(cacheKeyFiles, filesDb, TimeSpan.FromMinutes(1));

                files = (List<FileModel>)filesDb;
            }

            var cacheOffers = await _redisCache.GetCachedData(cacheKeyOffers);
            if (cacheOffers is not null)
            {
                offers = JsonConvert.DeserializeObject<List<OfferModel>>(cacheOffers);
            }
            else
            {
                var offersDb = await _readOffer.ReadAll(userId, 0, 5);
                await _redisCache.CacheData(cacheKeyOffers, offersDb, TimeSpan.FromMinutes(1));

                offers = (List<OfferModel>)offersDb;
            }

            foreach(var offer in offers)
            {
                list_offers.Add(new OfferObject
                { 
                    offer_id = offer.offer_id,
                    sender_id = offer.sender_id,
                    receiver_id = offer.receiver_id,
                    offer_type = offer.offer_type,
                    created_at = offer.created_at,
                    is_accepted = offer.is_accepted,
                });
            }

            bool IsOwner = userId.Equals(_userInfo.UserId);
            bool privateKey = userAndKeys.keys.private_key is not null;
            bool internalKey = userAndKeys.keys.internal_key is not null;
            bool receivedKey = userAndKeys.keys.received_key is not null;

            if (IsOwner)
            {
                var user = new
                {
                    id = userAndKeys.user.id,
                    username = userAndKeys.user.username,
                    email = userAndKeys.user.email,
                    role = userAndKeys.user.role,
                    is_blocked = userAndKeys.user.is_blocked
                };

                return StatusCode(200, new { user, IsOwner, keys = new { privateKey, internalKey, receivedKey }, files, offers = list_offers });
            }
            else
            {
                var user = new
                {
                    id = userAndKeys.user.id,
                    username = userAndKeys.user.username,
                    role = userAndKeys.user.role,
                    is_blocked = userAndKeys.user.is_blocked
                };

                return StatusCode(206, new { user, IsOwner, keys = new { privateKey, internalKey, receivedKey }, files, offers = list_offers });
            }
        }

        [HttpGet("data/only")]
        public async Task<IActionResult> GetOnlyUser()
        {
            var cacheKey = $"User_Data_{_userInfo.UserId}";

            try
            {
                var originalUser = new UserModel();

                bool clearCache = bool.TryParse(HttpContext.Session.GetString(Constants.CACHE_USER_DATA), out var parsedValue) ? parsedValue : true;
                if (clearCache)
                {
                    await _redisCache.DeleteCache(cacheKey);
                    HttpContext.Session.SetString(Constants.CACHE_USER_DATA, false.ToString());
                }

                var cache = await _redisCache.GetCachedData(cacheKey);
                if (cache is null)
                {
                    originalUser = await _readUser.ReadById(_userInfo.UserId, null);

                    await _redisCache.CacheData(cacheKey, originalUser, TimeSpan.FromMinutes(3));
                }
                else
                {
                    originalUser = JsonConvert.DeserializeObject<UserModel>(cache);
                }

                var user = new
                {
                    id = originalUser.id,
                    username = originalUser.username,
                    role = originalUser.role,
                    email = originalUser.email,
                    is_blocked = originalUser.is_blocked,
                    is_2fa_enabled = originalUser.is_2fa_enabled,
                };

                return StatusCode(200, new { user });
            }
            catch (UserException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser()
        {
            try
            {
                await _deleteUser.DeleteById(_userInfo.UserId, null);
                _tokenService.DeleteTokens();
                HttpContext.Session.Clear();

                return StatusCode(204);
            }
            catch (UserException)
            {
                return StatusCode(404);
            }
        }
    }

    public class OfferObject
    {
        public int offer_id { get; set; }
        public int sender_id { get; set; }
        public int receiver_id { get; set; }
        public string offer_type { get; set; }
        public DateTime created_at { get; set; }
        public bool is_accepted { get; set; }
    }
}
