using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Admin;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Services;
using webapi.Models;

namespace tests.Contollers_Tests.Admin
{
    public class SendEmailController_Test
    {
        [Fact]
        public async Task SendEmail_Success()
        {
            var notificationRepositoryMock = new Mock<IRepository<NotificationModel>>();
            var emailSenderMock = new Mock<IEmailSender>();
            var mapperMock = new Mock<IMapper>();

            mapperMock.Setup(m => m.Map<NotifyDTO, NotificationModel>(It.IsAny<NotifyDTO>())).Returns(new NotificationModel
            {
                message = string.Empty,
                message_header = string.Empty,
                priority = string.Empty,
                user_id = 1
            });

            var sendEmailController = new SendEmailController(notificationRepositoryMock.Object, mapperMock.Object, emailSenderMock.Object);
            var result = await sendEmailController.SendEmail(new NotifyDTO
            {
                message = string.Empty,
                message_header = string.Empty,
                priority = string.Empty,
                receiver_id = 1
            }, string.Empty, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(201, objectResult.StatusCode);
        }

        [Fact]
        public async Task SendEmail_NotificationNotCreated()
        {
            var notificationRepositoryMock = new Mock<IRepository<NotificationModel>>();
            var emailSenderMock = new Mock<IEmailSender>();
            var mapperMock = new Mock<IMapper>();

            notificationRepositoryMock.Setup(x => x.Add(It.IsAny<NotificationModel>(), null, CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(EntityNotCreatedException)));
            mapperMock.Setup(m => m.Map<NotifyDTO, NotificationModel>(It.IsAny<NotifyDTO>())).Returns(new NotificationModel
            {
                message = string.Empty,
                message_header = string.Empty,
                priority = string.Empty,
                user_id = 1
            });

            var sendEmailController = new SendEmailController(notificationRepositoryMock.Object, mapperMock.Object, emailSenderMock.Object);
            var result = await sendEmailController.SendEmail(new NotifyDTO
            {
                message = string.Empty,
                message_header = string.Empty,
                priority = string.Empty,
                receiver_id = 1
            }, string.Empty, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task SendEmail_MessageNotSent()
        {
            var notificationRepositoryMock = new Mock<IRepository<NotificationModel>>();
            var emailSenderMock = new Mock<IEmailSender>();
            var mapperMock = new Mock<IMapper>();

            emailSenderMock.Setup(x => x.SendMessage(It.IsAny<EmailDto>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(SmtpClientException)));
            mapperMock.Setup(m => m.Map<NotifyDTO, NotificationModel>(It.IsAny<NotifyDTO>())).Returns(new NotificationModel
            {
                message = string.Empty,
                message_header = string.Empty,
                priority = string.Empty,
                user_id = 1
            });

            var sendEmailController = new SendEmailController(notificationRepositoryMock.Object, mapperMock.Object, emailSenderMock.Object);
            var result = await sendEmailController.SendEmail(new NotifyDTO
            {
                message = string.Empty,
                message_header = string.Empty,
                priority = string.Empty,
                receiver_id = 1
            }, string.Empty, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
