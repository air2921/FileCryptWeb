using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DB.Abstractions;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Services.Abstractions;
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
        [RequestSizeLimit(50 * 1024 * 1024)]
        [ProducesResponseType(typeof(FileStreamResult), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> EncryptFile(
            [FromRoute] string type, [FromRoute] string operation,
            [FromQuery] bool sign, IFormFile file)
        {
            try
            {
                var key = await provider.GetCryptographyParams(type, operation);
                var encryptedFile = await provider.EncryptFile(new CryptographyOperationOptions
                {
                    Key = key,
                    Type = type,
                    Operation = operation,
                    File = file,
                    UserID = userInfo.UserId,
                    Username = sign ? userInfo.Username : null
                });

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.FILES_PREFIX}{userInfo.UserId}");

                return encryptedFile;
            }
            catch (ArgumentException)
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
