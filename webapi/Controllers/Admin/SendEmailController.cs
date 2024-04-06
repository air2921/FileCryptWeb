using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DB.Abstractions;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Localization;
using webapi.Models;
using webapi.Third_Party_Services.Abstractions;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/email/and/notification")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class SendEmailController(
        IRepository<NotificationModel> notificationRepository,
        IMapper mapper,
        IEmailSender emailSender) : ControllerBase
    {
        [HttpPost("send")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> SendEmail([FromBody] NotifyDTO notifyDTO, [FromQuery] string username, [FromQuery] string email)
        {
            try
            {
                var notificationModel = mapper.Map<NotifyDTO, NotificationModel>(notifyDTO);
                notificationModel.is_checked = false;
                notificationModel.send_time = DateTime.UtcNow;

                await notificationRepository.Add(notificationModel);

                await emailSender.SendMessage(new EmailDto
                {
                    username = username,
                    email = email,
                    subject = notifyDTO.message_header,
                    message = notifyDTO.message
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
