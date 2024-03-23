using Microsoft.AspNetCore.Mvc;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Account;

namespace webapi.Controllers.Account
{
    [Route("api/auth/recovery")]
    [ApiController]
    public class RecoveryController(
        IRecoveryHelpers recoveryHelper,
        [FromKeyedServices(ImplementationKey.ACCOUNT_RECOVERY_SERVICE)] IValidator validator,
        IRepository<UserModel> userRepository,
        IRepository<LinkModel> linkRepository,
        IEmailSender emailSender,
        IRedisCache redisCache,
        IFileManager fileManager,
        IGenerate generate) : ControllerBase
    {
        [HttpPost("unique/token")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> RecoveryAccount([FromQuery] string email)
        {
            try
            {
                var user = await userRepository.GetByFilter(query => query.Where(u => u.email.Equals(email.ToLowerInvariant())));
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                string token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString() + generate.GenerateKey();
                await recoveryHelper.CreateTokenTransaction(user, token);
                await emailSender.SendMessage(new EmailDto
                {
                    username = user.username,
                    email = email,
                    subject = EmailMessage.RecoveryAccountHeader,
                    message = EmailMessage.RecoveryAccountBody + $"{fileManager.GetReactAppUrl()}/auth/recovery?token={token}"
                });

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{user.id}");

                return StatusCode(201, new { message = Message.EMAIL_SENT });
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (SmtpClientException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("account")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 422)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> RecoveryAccountByToken([FromBody] RecoveryDTO recovery)
        {
            try
            {
                if (!validator.IsValid(recovery.password))
                    return StatusCode(400, new { message = Message.INVALID_FORMAT });

                var link = await linkRepository.GetByFilter(query => query.Where(l => l.u_token.Equals(recovery.token)));
                if (link is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (link.expiry_date < DateTime.UtcNow)
                {
                    await linkRepository.Delete(link.link_id);
                    return StatusCode(422, new { message = Message.FORBIDDEN });
                }

                var user = await userRepository.GetById(link.user_id);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });
                await recoveryHelper.RecoveryTransaction(user, recovery.token, recovery.password);

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{user.id}");
                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{user.id}");

                return StatusCode(200);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
