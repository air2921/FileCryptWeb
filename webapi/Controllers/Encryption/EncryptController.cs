﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.Controllers;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Services;

namespace webapi.Controllers.Encryption
{
    [Route("cryptography/{type}/encryption")]
    [ApiController]
    [Authorize]
    public class EncryptController : ControllerBase
    {
        private readonly ICryptographyControllerBase _cryptographyController;
        private readonly IUserInfo _userInfo;
        private readonly IEncrypt _encrypt;
        private readonly ICryptographyParamsProvider _cryptographyParams;

        public EncryptController(
            ICryptographyControllerBase cryptographyController,
            IUserInfo userInfo,
            IEncrypt encrypt,
            ICryptographyParamsProvider cryptographyParams)
        {
            _cryptographyController = cryptographyController;
            _userInfo = userInfo;
            _encrypt = encrypt;
            _cryptographyParams = cryptographyParams;
        }

        [HttpPost("encrypt")]
        [RequestSizeLimit(75 * 1024 * 1024)]
        public async Task<IActionResult> EncryptFile([FromRoute] string type, IFormFile file)
        {
            try
            {
                var param = await _cryptographyParams.GetCryptographyParams(type);

                return await _cryptographyController.EncryptFile(_encrypt.EncryptFileAsync, param.EncryptionKey!, file, _userInfo.UserId, type);
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
