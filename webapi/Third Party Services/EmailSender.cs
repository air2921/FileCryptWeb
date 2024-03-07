using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Net.Sockets;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces.Services;

namespace webapi.Third_Party_Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ISmtpClient _smtpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ISmtpClient smtpClient, IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _smtpClient = smtpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendMessage(EmailDto dto)
        {
            try
            {
                string Email = _configuration[App.EMAIL]!;

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("FileCrypt", Email));
                emailMessage.To.Add(new MailboxAddress(dto.username, dto.email));
                emailMessage.Subject = dto.subject;
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
                {
                    Text = dto.message
                };

                await _smtpClient.EmailSendAsync(emailMessage);
            }
            catch (SmtpClientException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw new SmtpClientException("Error sending message");
            }
        }

        public interface ISmtpClient
        {
            Task EmailSendAsync(MimeMessage message);
        }

        public class SmtpClientWrapper : ISmtpClient
        {
            private readonly SmtpClient _smtpClient;
            private readonly ILogger<SmtpClientWrapper> _logger;
            private readonly IConfiguration _configuration;

            public SmtpClientWrapper(IConfiguration configuration, ILogger<SmtpClientWrapper> logger)
            {
                _smtpClient = new SmtpClient();
                _configuration = configuration;
                _logger = logger;
            }

            public async Task EmailSendAsync(MimeMessage message)
            {
                try
                {
                    string Email = _configuration[App.EMAIL]!;
                    string Password = _configuration[App.EMAIL_PASSWORD]!;

                    await _smtpClient.ConnectAsync("smtp.yandex.ru", 587, SecureSocketOptions.Auto);
                    await _smtpClient.AuthenticateAsync(Email, Password);
                    await _smtpClient.SendAsync(message);
                }
                catch (AuthenticationException ex)
                {
                    _logger.LogError(ex.ToString(), nameof(EmailSendAsync));
                    throw new SmtpClientException("Error sending message");
                }
                catch (SocketException ex)
                {
                    _logger.LogError(ex.ToString(), nameof(EmailSendAsync));
                    throw new SmtpClientException("Error sending message");
                }
                finally
                {
                    await _smtpClient.DisconnectAsync(true);
                    _smtpClient.Dispose();
                }
            }
        }
    }
}
