using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Net.Sockets;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Third_Party_Services;
using static webapi.Third_Party_Services.EmailSender;

namespace tests.Third_Party_Services_Tests
{
    public class EmailSender_Test
    {
        [Fact]
        public async Task SendEmail_Success()
        {
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x["Email"]).Returns("email@example.com");
            configurationMock.Setup(x => x["EmailPassword"]).Returns("password");

            var loggerMock = new Mock<ILogger<EmailSender_Test>>();

            var emailSenderMock = new Mock<IEmailSender>();

            var emailDto = new EmailDto
            {
                username = "testuser",
                email = "testuser@example.com",
                subject = "testSubject",
                message = "testMessage"
            };

            emailSenderMock.Setup(x => x.SendMessage(emailDto))
                           .Returns(Task.CompletedTask);

            await emailSenderMock.Object.SendMessage(emailDto);
        }

        [Theory]
        [InlineData(typeof(AuthenticationException))]
        [InlineData(typeof(SocketException))]
        [InlineData(typeof(Exception))]
        public async Task SendEmail_Exception(Type exceptionType)
        {
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c["Email"]).Returns("email@example.com");
            configurationMock.Setup(c => c["EmailPassword"]).Returns("password");

            var loggerMock = new Mock<ILogger<EmailSender>>();
            var smtpClientMock = new Mock<ISmtpClient>();
            var emailManager = new EmailSender(smtpClientMock.Object, configurationMock.Object, loggerMock.Object);

            var emailDto = new EmailDto
            {
                username = "testuser",
                email = "testuser@example.com",
                subject = "testSubject",
                message = "testMessage"
            };

            smtpClientMock.Setup(client => client.EmailSendAsync(It.IsAny<MimeMessage>()))
                          .ThrowsAsync((Exception)Activator.CreateInstance(exceptionType));

            await Assert.ThrowsAsync<SmtpClientException>(() => emailManager.SendMessage(emailDto));
        }
    }
}
