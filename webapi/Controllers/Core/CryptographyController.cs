using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Base;
using webapi.Exceptions;
using webapi.Helpers;
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
        #region fields and constructor

        private readonly ICryptographyControllerBase _cryptographyController;
        private readonly ICypher _cypher;
        private readonly IUserInfo _userInfo;
        private readonly IRedisCache _redisCache;
        private readonly ICryptographyParamsProvider _cryptographyParams;

        public CryptographyController(
            ICryptographyControllerBase cryptographyController,
            ICypher cypher,
            IUserInfo userInfo,
            IRedisCache redisCache,
            ICryptographyParamsProvider cryptographyParams)
        {
            _cryptographyController = cryptographyController;
            _cypher = cypher;
            _userInfo = userInfo;
            _redisCache = redisCache;
            _cryptographyParams = cryptographyParams;
        }

        #endregion

        [HttpPost("{operation}")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(75 * 1024 * 1024)]
        [ProducesResponseType(typeof(FileStreamResult), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 415)]
        [ProducesResponseType(typeof(object), 422)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> EncryptFile([FromRoute] string type, [FromRoute] string operation, [FromQuery] bool validate, IFormFile file)
        {
            try
            {
                var param = await _cryptographyParams.GetCryptographyParams(type, operation);
                var encryptedFile = await _cryptographyController.EncryptFile(new CryptographyOperationOptions
                {
                    Key = param.EncryptionKey,
                    Type = type,
                    Operation = operation,
                    File = file,
                    UserID = _userInfo.UserId,
                    Username = validate ? _userInfo.Username : null
                });

                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.FILES_PREFIX}{_userInfo.UserId}");

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
    }
}
