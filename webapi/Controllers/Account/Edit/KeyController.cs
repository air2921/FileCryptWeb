using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/edit/keys")]
    [ApiController]
    [Authorize]
    public class KeyController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IUpdateKeys _updateKeys;
        private readonly IGenerateKey _generateKey;
        private readonly IRedisCache _redisCaching;
        private readonly IRedisKeys _redisKeys;
        private readonly IUserInfo _userInfo;
        private readonly ITokenService _tokenService;

        public KeyController(
            FileCryptDbContext dbContext,
            IUpdateKeys updateKeys,
            IGenerateKey generateKey,
            IRedisCache redisCaching,
            IRedisKeys redisKeys,
            IUserInfo userInfo,
            ITokenService tokenService)
        {
            _dbContext = dbContext;
            _updateKeys = updateKeys;
            _generateKey = generateKey;
            _redisCaching = redisCaching;
            _redisKeys = redisKeys;
            _userInfo = userInfo;
            _tokenService = tokenService;
        }

        [HttpPut("private/auto")]
        public async Task<IActionResult> UpdatePrivateKey()
        {
            try
            {
                var key = _generateKey.GenerateKey();
                var keyModel = new KeyModel { user_id = _userInfo.UserId, private_key = key };

                await _updateKeys.UpdatePrivateKey(keyModel);
                await _redisCaching.DeleteCache(_redisKeys.PrivateKey);

                return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated, private_key = key });
            }
            catch (UserException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404, new { message = ex.Message }); 
            }
        }

        [HttpPut("internal/auto")]
        public async Task<IActionResult> UpdatePersonalInternalKey()
        {
            try
            {
                var key = _generateKey.GenerateKey();
                var keyModel = new KeyModel { user_id = _userInfo.UserId, person_internal_key = key };

                await _updateKeys.UpdatePersonalInternalKey(keyModel);
                await _redisCaching.DeleteCache(_redisKeys.PersonalInternalKey);

                return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated, internal_key = key });
            }
            catch (UserException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpPut("received/clean")]
        public async Task<IActionResult> CleanReceivedInternalKey()
        {
            try
            {
                await _updateKeys.CleanReceivedInternalKey(_userInfo.UserId);
                await _redisCaching.DeleteCache(_redisKeys.ReceivedInternalKey);

                return StatusCode(200, new { message = AccountSuccessMessage.KeyRemoved });
            }
            catch (UserException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpPut("internal/own")]
        public async Task<IActionResult> UpdatePersonalInternalKeyToYourOwn(KeyModel keyModel)
        {
            try
            {
                var newKeyModel = new KeyModel { user_id = _userInfo.UserId, person_internal_key = keyModel.person_internal_key };
                await _updateKeys.UpdatePersonalInternalKey(newKeyModel);
                await _redisCaching.DeleteCache(_redisKeys.PersonalInternalKey);

                return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated, your_internal_key = keyModel.person_internal_key });
            }
            catch (UserException)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404);
            }
            catch (ArgumentException ex)
            {
                return StatusCode(422, new { message = ex.Message });
            }
        }

        [HttpPut("private/own")]
        public async Task<IActionResult> UpdatePrivateKeyToYourOwn(KeyModel keyModel)
        {
            try
            {
                var newKeyModel = new KeyModel { user_id = _userInfo.UserId, private_key = keyModel.private_key };
                await _updateKeys.UpdatePrivateKey(newKeyModel);
                await _redisCaching.DeleteCache(_redisKeys.PrivateKey);

                return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated, your_private_key = keyModel.private_key });
            }
            catch (UserException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return StatusCode(422, new { message = ex.Message });
            }
        }

        [HttpPut("received/from/offer")]
        public async Task<IActionResult> UpdateReceivedKey(int offerID)
        {
            var offer = await _dbContext.Offers.FirstOrDefaultAsync(o => o.offer_id == offerID);
            if (offer is null)
                return StatusCode(404, new { message = ExceptionOfferMessages.OfferNotFound });

            if (offer.receiver_id != _userInfo.UserId)
                return StatusCode(400);

            if (offer.is_accepted == true)
                return StatusCode(403, new { message = ExceptionOfferMessages.OfferIsAccepted });

            var targetUser = await _dbContext.Keys.FirstOrDefaultAsync(k => k.user_id == offer.receiver_id);
            if(targetUser is null)
            {
                _tokenService.DeleteTokens();
                return StatusCode(401);
            }

            targetUser.received_internal_key = offer.offer_body;
            offer.is_accepted = true;

            await _dbContext.SaveChangesAsync();

            return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated, key_from_id_offer = offerID });
        }
    }
}
