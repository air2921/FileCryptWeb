using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.Controllers;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.SQL.API;

namespace webapi.Controllers.Public_API
{
    [ApiController]
    [Route("api/public/cryptography/{type}/decryption")]
    public class API_DecryptController : ControllerBase
    {
        private readonly ICryptographyControllerBase _cryptographyController;
        private readonly IReadAPI _readAPI;
        private readonly IDecrypt _decrypt;

        public API_DecryptController(
            ICryptographyControllerBase cryptographyController,
            IReadAPI readAPI,
            IDecrypt decrypt)
        {
            _cryptographyController = cryptographyController;
            _readAPI = readAPI;
            _decrypt = decrypt;
        }

        [HttpPost("decrypt")]
        [RequestSizeLimit(75 * 1024 * 1024)]
        public async Task<IActionResult> DecryptFiles(
            [FromHeader(Name = "x-Encryption_Key")] string encryptionKey,
            [FromHeader(Name = "x-API_Key")] string apiKey,
            IFormFile file,
            [FromRoute] string type)
        {
            try
            {
                int userID = await _readAPI.ReadUserIdByApiKey(apiKey);

                return await _cryptographyController.EncryptFile(_decrypt.DecryptFileAsync, encryptionKey, file, userID, type);
            }
            catch (UserException ex)
            {
                return StatusCode(401, new { message = ex.Message });
            }
            catch (InvalidRouteException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
