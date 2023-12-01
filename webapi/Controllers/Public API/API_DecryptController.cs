using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Controllers;
using webapi.Interfaces.Cryptography;

namespace webapi.Controllers.Public_API
{
    [ApiController]
    [Route("api/public/cryptography/{type}/decryption")]
    public class API_DecryptController : ControllerBase
    {
        private readonly ICryptographyControllerBase _cryptographyController;
        private readonly FileCryptDbContext _dbContext;
        private readonly IDecrypt _decrypt;

        public API_DecryptController(
            ICryptographyControllerBase cryptographyController,
            FileCryptDbContext dbContext,
            IDecrypt decrypt)
        {
            _cryptographyController = cryptographyController;
            _dbContext = dbContext;
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
                var api = await _dbContext.API.FirstOrDefaultAsync(a => a.api_key == apiKey);
                if (api is null)
                    return StatusCode(401, new { message = "User not found" });

                return await _cryptographyController.EncryptFile(_decrypt.DecryptFileAsync, encryptionKey, file, api.user_id, type);
            }
            catch (InvalidRouteException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
