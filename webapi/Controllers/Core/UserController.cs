using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization.Exceptions;
using webapi.Models;

namespace webapi.Controllers.Core
{
    [Route("api/core/users")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IUserInfo _userInfo;
        private readonly ITokenService _tokenService;
        private readonly IRead<UserModel> _readUser;

        public UserController(FileCryptDbContext dbContext, IUserInfo userInfo, ITokenService tokenService, IRead<UserModel> readUser)
        {
            _dbContext = dbContext;
            _userInfo = userInfo;
            _tokenService = tokenService;
            _readUser = readUser;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser([FromRoute] int userId)
        {
            var user_keys_files = await _dbContext.Users
                .Where(u => u.id == userId)
                .GroupJoin(
                    _dbContext.Keys,
                    user => user.id,
                    keys => keys.user_id,
                    (user, keys) => new { user, keys })
                .SelectMany(
                    combined => _dbContext.Files
                        .Where(file => combined.keys.Select(k => k.user_id).Contains(file.user_id))
                        .DefaultIfEmpty(),
                    (combined, files) => new { combined.user, combined.keys, files })
                .OrderByDescending(combined => combined.files.operation_date)
                .Take(5)
                .ToListAsync();

            var offers = await _dbContext.Offers
                .Where(o => o.sender_id == userId || o.receiver_id == userId)
                .OrderByDescending(o => o.created_at)
                .Select(o => new { o.offer_id, o.sender_id, o.receiver_id, o.offer_type, o.created_at, o.is_accepted })
                .Take(5)
                .ToListAsync();

            if (!user_keys_files.Any())
            {
                if (userId.Equals(_userInfo.UserId))
                {
                    _tokenService.DeleteTokens();
                }
                return StatusCode(404, new { message = ExceptionUserMessages.UserNotFound });
            }

            var keys = user_keys_files.Select(u => u.keys.FirstOrDefault()).FirstOrDefault();
            var files = user_keys_files.Select(u => u.files).ToList();

            bool IsOwner = userId.Equals(_userInfo.UserId);
            bool privateKey = keys?.private_key is not null;
            bool internalKey = keys?.internal_key is not null;
            bool receivedKey = keys?.received_key is not null;

            if (userId.Equals(_userInfo.UserId))
            {
                var user = user_keys_files.Select(u => new { u.user.id, u.user.username, u.user.role, u.user.email }).FirstOrDefault();

                return StatusCode(200, new { user, IsOwner, keys = new { privateKey, internalKey, receivedKey }, files, offers });
            }
            else
            {
                var user = user_keys_files.Select(u => new { u.user.id, u.user.username, u.user.role }).FirstOrDefault();

                return StatusCode(206, new { user, IsOwner, keys = new { privateKey, internalKey, receivedKey }, files, offers });
            }
        }

        [HttpGet("data/only")]
        public async Task<IActionResult> GetOnlyUser()
        {
            try
            {
                var originalUser = await _readUser.ReadById(_userInfo.UserId, null);
                var user = new
                {
                    id = originalUser.id,
                    username = originalUser.username,
                    role = originalUser.role,
                    email = originalUser.email
                };

                return StatusCode(200, new { user });
            }
            catch (UserException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
