using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [Route("api/core/users")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        #region fields and constructor

        private readonly FileCryptDbContext _dbContext;
        private readonly IRepository<OfferModel> _offerRepository;
        private readonly IRepository<FileModel> _fileRepository;
        private readonly IRepository<UserModel> _userRepository;
        private readonly IRedisCache _redisCache;
        private readonly IUserInfo _userInfo;
        private readonly ITokenService _tokenService;

        public UserController(
            FileCryptDbContext dbContext,
            IRepository<OfferModel> offerRepository,
            IRepository<FileModel> fileRepository,
            IRepository<UserModel> userRepository,
            IRedisCache redisCache,
            IUserInfo userInfo,
            ITokenService tokenService)
        {
            _dbContext = dbContext;
            _offerRepository = offerRepository;
            _fileRepository = fileRepository;
            _userRepository = userRepository;
            _redisCache = redisCache;
            _userInfo = userInfo;
            _tokenService = tokenService;
        }

        #endregion

        [HttpGet("{userId}/{username}")]
        public async Task<IActionResult> GetUser([FromRoute] int userId, [FromRoute] string username)
        {
            try
            {
                var user = await GetUserAndKeys(username, userId);
                var files = await GetFiles(userId);
                var offers = await GetOffers(userId);

                return StatusCode(200, new { user = user.user, isOwner = user.isOwner, keys = user.keys, files, offers });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("data/only")]
        public async Task<IActionResult> GetOnlyUser()
        {
            try
            {
                var cacheKey = $"{ImmutableData.USER_DATA_PREFIX}{_userInfo.UserId}";
                var user = new UserModel();

                var cache = await _redisCache.GetCachedData(cacheKey);
                if (cache is null)
                {
                    user = await _userRepository.GetById(_userInfo.UserId);
                    if (user is null)
                        return StatusCode(404);

                    await _redisCache.CacheData(cacheKey, user, TimeSpan.FromMinutes(3));
                }
                else
                    user = JsonConvert.DeserializeObject<UserModel>(cache);

                user.password = string.Empty;

                return StatusCode(200, new { user });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser()
        {
            try
            {
                await _userRepository.Delete(_userInfo.UserId);
                _tokenService.DeleteTokens();
                HttpContext.Session.Clear();

                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("find")]
        public async Task<IActionResult> FindUser([FromQuery] string? username, [FromQuery] int userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) && userId == 0)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (!string.IsNullOrWhiteSpace(username) && userId == 0)
                    return StatusCode(200, new { users = await GetUserList(username) });

                if (string.IsNullOrWhiteSpace(username) && userId != 0)
                    return StatusCode(200, new { user = await GetUserById(userId) });

                return StatusCode(404, new { message = Message.NOT_FOUND });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [Helper]
        private async Task<UserKeysObject> GetUserAndKeys(string username, int userId)
        {
            var userAndKeys = await _dbContext.Users
                .Where(u => u.id == userId && u.username == username)
                .Join(_dbContext.Keys, user => user.id, keys => keys.user_id, (user, keys) => new { user, keys })
                .FirstOrDefaultAsync();

            if (userAndKeys is null)
                throw new ArgumentNullException(Message.NOT_FOUND);

            bool IsOwner = userId.Equals(_userInfo.UserId);
            bool privateKey = userAndKeys.keys.private_key is not null;
            bool internalKey = userAndKeys.keys.internal_key is not null;
            bool receivedKey = userAndKeys.keys.received_key is not null;

            var user = new
            {
                id = userAndKeys.user.id,
                username = userAndKeys.user.username,
                email = IsOwner ? userAndKeys.user.email : null,
                role = userAndKeys.user.role,
                is_blocked = userAndKeys.user.is_blocked
            };

            var keys = new
            {
                privateKey,
                internalKey,
                receivedKey
            };

            return new UserKeysObject { user = user, isOwner = IsOwner, keys = keys };
        }

        [Helper]
        private async Task<IEnumerable<OfferModel>> GetOffers(int userId)
        {
            var cacheKeyOffers = $"Profile_Offers_{userId}";
            var offers = new List<OfferModel>();

            var cacheOffers = await _redisCache.GetCachedData(cacheKeyOffers);
            if (cacheOffers is null)
            {
                var offersDb = await _offerRepository.GetAll
                    (query => query.Where(o => o.receiver_id.Equals(userId) || o.sender_id.Equals(userId))
                    .OrderByDescending(o => o.created_at).Skip(0).Take(5));

                await _redisCache.CacheData(cacheKeyOffers, offersDb, TimeSpan.FromMinutes(1));

                offers = (List<OfferModel>)offersDb;
            }
            else
                offers = JsonConvert.DeserializeObject<List<OfferModel>>(cacheOffers);

            foreach (var offer in offers)
            {
                offer.offer_header = string.Empty;
                offer.offer_body = string.Empty;
            }

            return offers;
        }

        [Helper]
        private async Task<IEnumerable<FileModel>> GetFiles(int userId)
        {
            var cacheKeyFiles = $"Profile_Files_{userId}";

            var cacheFiles = await _redisCache.GetCachedData(cacheKeyFiles);
            if (cacheFiles is null)
            {
                var filesDb = await _fileRepository.GetAll
                    (query => query.Where(f => f.user_id.Equals(userId)).OrderByDescending(f => f.operation_date).Skip(0).Take(5));

                await _redisCache.CacheData(cacheKeyFiles, filesDb, TimeSpan.FromMinutes(1));

                return filesDb;
            }
            else
                return JsonConvert.DeserializeObject<List<FileModel>>(cacheFiles);
        }

        [Helper]
        private async Task<IEnumerable<UserModel>> GetUserList(string? username)
        {
            var users = await _userRepository.GetAll(query => query.Where(u => u.username.Equals(username)));

            foreach (var user in users)
            {
                user.password = string.Empty;
                user.email = string.Empty;
            }

            return users;

            throw new ArgumentException(Message.NOT_FOUND);
        }

        [Helper]
        private async Task<UserModel> GetUserById(int userId)
        {
            var user = await _userRepository.GetById(userId);
            if (user is null)
                throw new ArgumentException(Message.NOT_FOUND);

            return user;
        }

        [AuxiliaryObject]
        private class UserKeysObject
        {
            public object user { get; set; }
            public object keys { get; set; }
            public bool isOwner { get; set; }
        }
    }
}
