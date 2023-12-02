using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Interfaces.Services;
using webapi.Localization.English;

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

        public UserController(FileCryptDbContext dbContext, IUserInfo userInfo, ITokenService tokenService)
        {
            _dbContext = dbContext;
            _userInfo = userInfo;
            _tokenService = tokenService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser([FromRoute] int id)
        {
            var user_keys_files = await _dbContext.Users
                .Where(u => u.id == id)
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
                .ToListAsync();

            if (!user_keys_files.Any() && id == _userInfo.UserId)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404);
            }

            if (!user_keys_files.Any())
                return StatusCode(404, new { message = ExceptionUserMessages.UserNotFound });


            var keys = user_keys_files.Select(u => u.keys.FirstOrDefault()).FirstOrDefault();
            var files = user_keys_files.Select(u => u.files).ToList();

            if (id == _userInfo.UserId)
            {
                var user = user_keys_files.Select(u => new
                {
                    u.user.id,
                    u.user.username,
                    u.user.role,
                    u.user.email
                }).FirstOrDefault();

                return StatusCode(200, new { user, keys, files });
            }
            else
            {
                var user = user_keys_files.Select(u => new
                {
                    u.user.id,
                    u.user.username,
                    u.user.role
                }).FirstOrDefault();

                bool hasPrivate = true;
                bool hasInternal = true;
                bool hasReceived = true;

                if (keys.private_key is null)
                    hasPrivate = false;
                if (keys.person_internal_key is null)
                    hasInternal = false;
                if (keys.received_internal_key is null)
                    hasReceived = false;

                return StatusCode(206, new
                {
                    user,
                    keys = new
                    {
                        hasPrivate,
                        hasInternal,
                        hasReceived
                    },
                    files
                });
            }
        }
    }
}
