using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/email/and/notification")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    [ValidateAntiForgeryToken]
    public class SendEmailController : ControllerBase
    {
        private readonly ILogger<SendEmailController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ICreate<NotificationModel> _createNotification;
        private readonly IUserInfo _userInfo;

        public SendEmailController(ILogger<SendEmailController> logger, IEmailSender emailSender, ICreate<NotificationModel> createNotification, IUserInfo userInfo)
        {
            _logger = logger;
            _emailSender = emailSender;
            _createNotification = createNotification;
            _userInfo = userInfo;
        }

        [HttpPost("send")]
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

                _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} sent message via work email to {username}#{notifyDTO.receiver_id} on {email}");

                await _createNotification.Create(new NotificationModel
                {
                    receiver_id = notifyDTO.receiver_id,
                    message_header = "You have a notification from administrator",
                    message = notifyDTO.message,
                    send_time = DateTime.UtcNow,
                    priority = notifyDTO.priority,
                    is_checked = false
                });
                _logger.LogInformation($"Created notification. Sender: {_userInfo.Username}#{_userInfo.UserId}. Receiver:{username}#{notifyDTO.receiver_id} ");

                return StatusCode(201, new { message = SuccessMessage.SuccessEmailSendedAndCreatedNotification });
            }
            catch (UserException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (AuthenticationException)
            {
                return StatusCode(500, new { message = AccountErrorMessage.Error });
            }
            catch (SocketException)
            {
                return StatusCode(500, new { message = AccountErrorMessage.Error });
            }
        }
    }
}
