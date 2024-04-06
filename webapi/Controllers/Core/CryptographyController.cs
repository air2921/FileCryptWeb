using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Services.Core;

namespace webapi.Controllers.Core
{
    [Route("api/core/cryptography/{type}")]
    [ApiController]
    [Authorize]
    public class CryptographyController(
        IUserInfo userInfo,
        IRedisCache redisCache,
        ICryptographyProvider provider) : ControllerBase
    {
        [HttpPost("{operation}")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(75 * 1024 * 1024)]
        [ProducesResponseType(typeof(FileStreamResult), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 422)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> EncryptFile(
            [FromRoute] string type, [FromRoute] string operation,
            [FromQuery] bool validate, IFormFile file)
        {
            try
            {
                var param = await provider.GetCryptographyParams(type, operation);
                var encryptedFile = await provider.EncryptFile(new CryptographyOperationOptions
                {
                    Key = param.EncryptionKey,
                    Type = type,
                    Operation = operation,
                    File = file,
                    UserID = userInfo.UserId,
                    Username = validate ? userInfo.Username : null
                });

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.FILES_PREFIX}{userInfo.UserId}");

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
