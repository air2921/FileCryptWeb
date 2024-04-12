using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using services.Abstractions;
using services.Exceptions;
using services.Sender.DTO;
using System.Net.Sockets;

namespace services.Sender
{
    public class EmailSender(EmailSender.ISmtpClient smtpClient, ILogger<EmailSender> logger) : IEmailSender
    {
        public string Email { get; set; }

        public async Task SendMessage(EmailDto dto)
        {
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("FileCrypt", Email));
                emailMessage.To.Add(new MailboxAddress(dto.username, dto.email));
                emailMessage.Subject = dto.subject;
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
                {
                    Text = dto.message
                };

                await smtpClient.EmailSendAsync(emailMessage);
            }
            catch (SmtpClientException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                throw new SmtpClientException("Error sending message");
            }
        }

        public interface ISmtpClient
        {
            Task EmailSendAsync(MimeMessage message);
        }

        public class SmtpClientWrapper : ISmtpClient
        {
            public string Email { get; set; }
            public string Password { get; set; }

            private readonly SmtpClient _smtpClient;
            private readonly ILogger<SmtpClientWrapper> _logger;

            public SmtpClientWrapper(ILogger<SmtpClientWrapper> logger)
            {
                _smtpClient = new SmtpClient();
                _logger = logger;
            }

            public async Task EmailSendAsync(MimeMessage message)
            {
                try
                {

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
