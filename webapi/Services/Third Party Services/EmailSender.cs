using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Net.Sockets;
using webapi.DTO;
using webapi.Interfaces.Services;

namespace webapi.Services.Third_Party_Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendMessage(EmailDto dto)
        {
            try
            {
                string Email = _configuration[App.EMAIL]!;
                string Password = _configuration[App.EMAIL_PASSWORD]!;

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("FileCrypt", Email));
                emailMessage.To.Add(new MailboxAddress(dto.username, dto.email));
                emailMessage.Subject = dto.subject;
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
                {
                    Text = dto.message
                };

                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.yandex.ru", 587, SecureSocketOptions.Auto);
                await client.AuthenticateAsync(Email, Password);
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
            catch (AuthenticationException ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
            catch (SocketException ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }
    }
}
