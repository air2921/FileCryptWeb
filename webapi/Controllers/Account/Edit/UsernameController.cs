using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/edit/username")]
    [ApiController]
    [Authorize]
    [ValidateAntiForgeryToken]
    public class UsernameController : ControllerBase
    {
        private readonly IRepository<UserModel> _userRepository;
        private readonly ILogger<UsernameController> _logger;
        private readonly IUserInfo _userInfo;
        private readonly ITokenService _tokenService;

        public UsernameController(
            IRepository<UserModel> userRepository,
            ILogger<UsernameController> logger,
            IUserInfo userInfo,
            ITokenService tokenService)
        {
            _userRepository = userRepository;
            _logger = logger;
            _userInfo = userInfo;
            _tokenService = tokenService;
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUsername([FromQuery] string username)
        {
            try
            {
                if (!Regex.IsMatch(username, Validation.Username))
                    return StatusCode(400, new { message = AccountErrorMessage.InvalidFormatUsername });

                var user = await _userRepository.GetById(_userInfo.UserId);
                if (user is null)
                {
                    _tokenService.DeleteTokens();
                    _logger.LogWarning("Tokens was deleted");
                    return StatusCode(404);
                }

                user.username = username;

                await _userRepository.Update(user);
                _logger.LogInformation($"username was updated in db. {username}#{_userInfo.UserId}");

                await _tokenService.UpdateJwtToken();
                _tokenService.DeleteUserDataSession();
                _logger.LogInformation("jwt with a new claims was updated");
                HttpContext.Session.SetString(Constants.CACHE_USER_DATA, true.ToString());

                return StatusCode(200, new { message = AccountSuccessMessage.UsernameUpdated });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Error when trying to update jwt.\nTrying delete tokens");
                _tokenService.DeleteTokens();
                _logger.LogWarning("Tokens was deleted");
                return StatusCode(206, new { message = ex.Message });
            }
        }
    }
}
