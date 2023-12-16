using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers.Admin.Manage_Encryption_Keys
{
    [Route("api/admin/keys")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    public class ReadKeysController : ControllerBase
    {
        private readonly IUserInfo _userInfo;
        private readonly IRead<KeyModel> _read;
        private readonly IDecryptKey _decryptKey;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReadKeysController> _logger;
        private readonly FileCryptDbContext _dbContext;
        private readonly byte[] secretKey;

        public ReadKeysController(
            IUserInfo userInfo,
            IRead<KeyModel> read,
            IDecryptKey decryptKey,
            IConfiguration configuration,
            ILogger<ReadKeysController> logger,
            FileCryptDbContext dbContext)
        {
            _userInfo = userInfo;
            _read = read;
            _decryptKey = decryptKey;
            _configuration = configuration;
            _logger = logger;
            _dbContext = dbContext;
            secretKey = Convert.FromBase64String(_configuration[App.appKey]!);
        }

        [HttpGet("all/{userId}")]
        public async Task<IActionResult> AllKeys([FromRoute] int userId)
        {
            try
            {
                HashSet<string> decryptedKeys = new();

                var userKeys = await _read.ReadById(userId, true);

                string?[] encryptionKeys =
                {
                    userKeys.private_key,
                    userKeys.person_internal_key,
                    userKeys.received_internal_key
                };

                foreach (string? encryptedKey in encryptionKeys)
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
                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} get keys user#{userId}");
                return StatusCode(200, new { keys = decryptedKeys });
            }
            catch (UserException ex)
            {
                _logger.LogWarning($"user#{userId} not exists");
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
