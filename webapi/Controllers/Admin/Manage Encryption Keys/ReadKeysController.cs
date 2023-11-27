using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.SQL.Keys;
using webapi.Localization.English;

namespace webapi.Controllers.Admin.Manage_Encryption_Keys
{
    [Route("api/admin/keys/get")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    public class ReadKeysController : ControllerBase
    {
        private readonly IReadKeys _readKeys;
        private readonly IDecryptKey _decryptKey;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReadKeysController> _logger;
        private readonly FileCryptDbContext _dbContext;
        private readonly byte[] secretKey;

        public ReadKeysController(
            IReadKeys readKeys,
            IDecryptKey decryptKey,
            IConfiguration configuration,
            ILogger<ReadKeysController> logger,
            FileCryptDbContext dbContext)
        {
            _readKeys = readKeys;
            _decryptKey = decryptKey;
            _configuration = configuration;
            _logger = logger;
            _dbContext = dbContext;
            secretKey = Convert.FromBase64String(_configuration["FileCryptKey"]!);
        }

        [HttpGet("allKeys")]
        public async Task<IActionResult> AllKeys(int userId)
        {
            HashSet<string> decryptedKeys = new();

            var keysDB = await _dbContext.Keys.FirstOrDefaultAsync(k => k.user_id == userId);
            if (keysDB is null)
                return StatusCode(404, new { message = ExceptionKeyMessages.RecordKeysNotFound });

            string?[] encryptedKeys = { keysDB.private_key, keysDB.person_internal_key, keysDB.received_internal_key };

            foreach (string? encryptedKey in encryptedKeys)
            {
                try
                {
                    if (encryptedKey is null)
                        continue;

                    decryptedKeys.Add(await _decryptKey.DecryptionKeyAsync(encryptedKey, secretKey));
                }
                catch (CryptographicException ex)
                {
                    _logger.LogCritical(ex.ToString(), nameof(AllKeys));
                    continue;
                }
            }
            return StatusCode(200, new { keys = decryptedKeys.ToArray() });
        }

        [HttpGet("privateKey")]
        public async Task<IActionResult> Private(int userId)
        {
            try
            {
                string encryptedKey = await _readKeys.ReadPrivateKey(userId);

                return StatusCode(200, new { key = await _decryptKey.DecryptionKeyAsync(encryptedKey, secretKey) });
            }
            catch (UserException ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
            catch (CryptographicException)
            {
                return StatusCode(500, new { message = ErrorMessage.BadCryptographyData });
            }
        }

        [HttpGet("internalKey")]
        public async Task<IActionResult> Internal(int userId)
        {
            try
            {
                string encryptedKey = await _readKeys.ReadPersonalInternalKey(userId);

                return StatusCode(200, new { key = await _decryptKey.DecryptionKeyAsync(encryptedKey, secretKey) });
            }
            catch (KeyException ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
            catch (UserException ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
            catch (CryptographicException)
            {
                return StatusCode(500, new { message = ErrorMessage.BadCryptographyData });
            }
        }

        [HttpGet("receivedKey")]
        public async Task<IActionResult> Received(int userId)
        {
            try
            {
                string encryptedKey = await _readKeys.ReadReceivedInternalKey(userId);

                return StatusCode(200, new { key = await _decryptKey.DecryptionKeyAsync(encryptedKey, secretKey) });
            }
            catch (KeyException ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
            catch (UserException ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
            catch (CryptographicException)
            {
                return StatusCode(500, new { message = ErrorMessage.BadCryptographyData });
            }
        }
    }
}
