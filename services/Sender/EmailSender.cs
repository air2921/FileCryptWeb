using application.Abstractions.TP_Services;
using application.DTO.Inner;
using domain.Exceptions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Net.Sockets;

namespace services.Sender
{
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
    public class EmailSender : IEmailSender
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;
        private readonly EmailSender.ISmtpClient _smtpClient;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger, EmailSender.ISmtpClient smtpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _smtpClient = smtpClient;
        }

        public async Task SendMessage(EmailDto dto)
        {
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("FileCrypt", _configuration["Email"]));
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

#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
            public SmtpClientWrapper(ILogger<SmtpClientWrapper> logger, IConfiguration configuration)
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
            {
                _smtpClient = new SmtpClient();
                _configuration = configuration;
                _logger = logger;
            }

            public async Task EmailSendAsync(MimeMessage message)
            {
                try
                {
                    await _smtpClient.ConnectAsync("smtp.yandex.ru", 587, SecureSocketOptions.Auto);
                    await _smtpClient.AuthenticateAsync(_configuration["Email"], _configuration["EmailPassword"]);
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
