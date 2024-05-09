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
    public class EmailSender(IConfiguration configuration, ILogger<EmailSender> logger, EmailSender.ISmtpClient smtpClient) : IEmailSender
    {
        public async Task SendMessage(EmailDto dto)
        {
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("FileCrypt", configuration["Email"]));
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

        public class SmtpClientWrapper(ILogger<EmailSender.SmtpClientWrapper> logger, IConfiguration configuration) : ISmtpClient
        {
            private readonly SmtpClient _smtpClient = new();

            public async Task EmailSendAsync(MimeMessage message)
            {
                try
                {
                    await _smtpClient.ConnectAsync("smtp.yandex.ru", 587, SecureSocketOptions.Auto);
                    await _smtpClient.AuthenticateAsync(configuration["Email"], configuration["EmailPassword"]);
                    await _smtpClient.SendAsync(message);
                }
                catch (AuthenticationException ex)
                {
                    logger.LogError(ex.ToString(), nameof(EmailSendAsync));
                    throw new SmtpClientException("Error sending message");
                }
                catch (SocketException ex)
                {
                    logger.LogError(ex.ToString(), nameof(EmailSendAsync));
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
