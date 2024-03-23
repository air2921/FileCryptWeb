using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;

        public SendEmailController(
            IRepository<NotificationModel> notificationRepository,
            IMapper mapper,
            IEmailSender emailSender)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
            _emailSender = emailSender;
        }

        #endregion

        [HttpPost("send")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> SendEmail([FromBody] NotifyDTO notifyDTO, [FromQuery] string username, [FromQuery] string email)
        {
            try
            {
                var notificationModel = _mapper.Map<NotifyDTO, NotificationModel>(notifyDTO);
                notificationModel.is_checked = false;
                notificationModel.send_time = DateTime.UtcNow;

                await _notificationRepository.Add(notificationModel);

                await _emailSender.SendMessage(new EmailDto
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
