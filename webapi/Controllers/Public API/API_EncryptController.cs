using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Controllers;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.Controllers.Public_API
{
    [ApiController]
    [EnableCors("AllowOriginAPI")]
    [Route("api/public/cryptography/{type}/encryption")]
    public class API_EncryptController : ControllerBase
    {
        private readonly ICryptographyControllerBase _cryptographyController;
        private readonly FileCryptDbContext _dbContext;
        private readonly IEncrypt _encrypt;

        public API_EncryptController(
            ICryptographyControllerBase cryptographyController,
            FileCryptDbContext dbContext,
            IEncrypt encrypt)
        {
            _cryptographyController = cryptographyController;
            _dbContext = dbContext;
            _encrypt = encrypt;
        }

        [HttpPost("encrypt")]
        [RequestSizeLimit(75 * 1024 * 1024)]
        public async Task<IActionResult> EncryptFiles(
            [FromHeader(Name = "x-Encryption_Key")] string encryptionKey,
            [FromHeader(Name = "x-API_Key")] string apiKey,
            IFormFile file,
            [FromRoute] string type)
        {
            try
            {
                var api = await _dbContext.API.FirstOrDefaultAsync(a => a.api_key == apiKey);
                if(api is null)
                    return StatusCode(401, new { message = "User not found" });

                return await _cryptographyController.EncryptFile(_encrypt.EncryptFileAsync, encryptionKey, file, api.user_id, type);
            }
            catch (InvalidRouteException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
