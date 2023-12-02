using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.Controllers;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Services;

namespace webapi.Controllers.Encryption
{
    [Route("api/cryptography/{type}")]
    [ApiController]
    [Authorize]
    public class DecryptController : ControllerBase
    {
        private readonly ICryptographyControllerBase _cryptographyController;
        private readonly IUserInfo _userInfo;
        private readonly IDecrypt _decrypt;
        private readonly ICryptographyParamsProvider _cryptographyParams;

        public DecryptController(
            ICryptographyControllerBase cryptographyController,
            IUserInfo userInfo,
            IDecrypt decrypt,
            ICryptographyParamsProvider cryptographyParams)
        {
            _cryptographyController = cryptographyController;
            _userInfo = userInfo;
            _decrypt = decrypt;
            _cryptographyParams = cryptographyParams;
        }

        [HttpPost("decrypt")]
        [RequestSizeLimit(75 * 1024 * 1024)]
        public async Task<IActionResult> DecryptFile([FromRoute] string type, IFormFile file)
        {
            try
            {
                var param = await _cryptographyParams.GetCryptographyParams(type);

                return await _cryptographyController.EncryptFile(_decrypt.DecryptFileAsync, param.EncryptionKey!, file, _userInfo.UserId, type);
            }
            catch (UserException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (KeyException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (InvalidRouteException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
