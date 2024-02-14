using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.Controllers;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;

namespace webapi.Controllers.Core
{
    [Route("api/core/cryptography/{type}")]
    [ApiController]
    [Authorize]
    public class CryptographyController : ControllerBase
    {
        private readonly ICryptographyControllerBase _cryptographyController;
        private readonly IUserInfo _userInfo;
        private readonly IRedisCache _redisCache;
        private readonly IEncrypt _encrypt;
        private readonly IDecrypt _decrypt;
        private readonly ICryptographyParamsProvider _cryptographyParams;

        public CryptographyController(
            ICryptographyControllerBase cryptographyController,
            IUserInfo userInfo,
            IRedisCache redisCache,
            IEncrypt encrypt,
            IDecrypt decrypt,
            ICryptographyParamsProvider cryptographyParams)
        {
            _cryptographyController = cryptographyController;
            _userInfo = userInfo;
            _redisCache = redisCache;
            _encrypt = encrypt;
            _decrypt = decrypt;
            _cryptographyParams = cryptographyParams;
        }

        [HttpPost("encrypt")]
        [RequestSizeLimit(75 * 1024 * 1024)]
        public async Task<IActionResult> EncryptFile([FromRoute] string type, IFormFile file)
        {
            try
            {
                var param = await _cryptographyParams.GetCryptographyParams(type);

                var encryptedFile = await _cryptographyController.EncryptFile(_encrypt.EncryptFileAsync, param.EncryptionKey, file, _userInfo.UserId, type);
                await _redisCache.DeteteCacheByKeyPattern($"Files_{_userInfo.UserId}");

                return encryptedFile;
            }
            catch (ArgumentNullException)
            {
                return StatusCode(404, new { message = "Encryption key not found" });
            }
            catch (InvalidRouteException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpPost("decrypt")]
        [RequestSizeLimit(75 * 1024 * 1024)]
        public async Task<IActionResult> DecryptFile([FromRoute] string type, IFormFile file)
        {
            try
            {
                var param = await _cryptographyParams.GetCryptographyParams(type);

                var decryptedFile = await _cryptographyController.EncryptFile(_decrypt.DecryptFileAsync, param.EncryptionKey, file, _userInfo.UserId, type);
                await _redisCache.DeteteCacheByKeyPattern($"Files_{_userInfo.UserId}");

                return decryptedFile;
            }
            catch (ArgumentNullException)
            {
                return StatusCode(404, new { message = "Encryption key not found" });
            }
            catch (InvalidRouteException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
