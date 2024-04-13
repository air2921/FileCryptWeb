using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using services.Abstractions;
using services.DTO;
using services.Exceptions;
using System.Net.Sockets;

namespace services.Sender
{
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
    public class EmailSender(EmailSender.ISmtpClient smtpClient, ILogger<EmailSender> logger) : IEmailSender
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
    {
        public string Email { private get; set; }

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
            public string Email { private get; set; }
            public string Password { private get; set; }

            private readonly SmtpClient _smtpClient;
            private readonly ILogger<SmtpClientWrapper> _logger;

#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
            public SmtpClientWrapper(ILogger<SmtpClientWrapper> logger)
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
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
