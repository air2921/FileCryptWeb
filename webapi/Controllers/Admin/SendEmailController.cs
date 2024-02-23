using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/email/and/notification")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class SendEmailController : ControllerBase
    {
        #region fields and constructor

        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly ILogger<SendEmailController> _logger;
        private readonly IEmailSender _emailSender;

        public SendEmailController(
            IRepository<NotificationModel> notificationRepository,
            ILogger<SendEmailController> logger,
            IEmailSender emailSender)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
            _emailSender = emailSender;
        }

        #endregion

        [HttpPost("send")]
        [XSRFProtection]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> SendEmail([FromBody] NotifyDTO notifyDTO, [FromQuery] string username, [FromQuery] string email)
        {
            try
            {
                await _emailSender.SendMessage(new EmailDto
                {
                    username = username,
                    email = email,
                    subject = notifyDTO.message_header,
                    message = notifyDTO.message
                });

                await _notificationRepository.Add(new NotificationModel
                {
                    user_id = notifyDTO.receiver_id,
                    message_header = "You have a notification from administrator",
                    message = notifyDTO.message,
                    send_time = DateTime.UtcNow,
                    priority = notifyDTO.priority,
                    is_checked = false
                });

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
        }
    }
}
