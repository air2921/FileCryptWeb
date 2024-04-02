using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Admin;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Services;
using webapi.Models;

namespace tests.Controllers_Tests.Admin
{
    public class SendEmailController_Test
    {
        [Fact]
        public async Task SendEmail_Success()
        {
            var notificationRepositoryMock = new Mock<IRepository<NotificationModel>>();
            var emailSenderMock = new Mock<IEmailSender>();
            var mapperMock = new Mock<IMapper>();
            var ntfModel = new NotificationModel
            {
                message = string.Empty,
                message_header = string.Empty,
                priority = string.Empty,
                user_id = 1
            };

            mapperMock.Setup(m => m.Map<NotifyDTO, NotificationModel>(It.IsAny<NotifyDTO>())).Returns(ntfModel);

            var sendEmailController = new SendEmailController(notificationRepositoryMock.Object, mapperMock.Object, emailSenderMock.Object);
            var result = await sendEmailController.SendEmail(new NotifyDTO
            {
                message = string.Empty,
                message_header = string.Empty,
                priority = string.Empty,
                receiver_id = 1
            }, string.Empty, string.Empty);

            emailSenderMock.Verify(x => x.SendMessage(It.IsAny<EmailDto>()), Times.Once);
            notificationRepositoryMock.Verify(x => x.Add(ntfModel, null, CancellationToken.None), Times.Once);
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
                .ThrowsAsync(new EntityNotCreatedException());
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
                .ThrowsAsync(new SmtpClientException());
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
