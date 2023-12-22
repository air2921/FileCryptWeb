using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
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
        private readonly IEmailSender<UserModel> _emailSender;
        private readonly ICreate<NotificationModel> _createNotification;
        private readonly IUserInfo _userInfo;

        public SendEmailController(ILogger<SendEmailController> logger, IEmailSender<UserModel> emailSender, ICreate<NotificationModel> createNotification, IUserInfo userInfo)
        {
            _logger = logger;
            _emailSender = emailSender;
            _createNotification = createNotification;
            _userInfo = userInfo;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] NotificationModel notificationModel, [FromQuery] string username, [FromQuery] string email)
        {
            try
            {
                var userModel = new UserModel { username = username, email = email };

                await _emailSender.SendMessage(userModel, notificationModel.message_header, notificationModel.message);
                

                var newNotificationModel = new NotificationModel
                {
                    sender_id = _userInfo.UserId,
                    receiver_id = notificationModel.receiver_id,
                    message_header = "You have a notification from administrator",
                    message = notificationModel.message,
                    send_time = DateTime.UtcNow,
                    priority = notificationModel.priority,
                    is_checked = false
                };

                _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} sent message via work email to {username}#{notificationModel.receiver_id} on {email}");

                await _createNotification.Create(newNotificationModel);
                _logger.LogInformation($"Created notification. Sender: {_userInfo.Username}#{_userInfo.UserId}. Receiver:{username}#{notificationModel.receiver_id} ");

                return StatusCode(201, new { message = SuccessMessage.SuccessEmailSendedAndCreatedNotification, sended_notification = newNotificationModel });
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
