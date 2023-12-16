using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Net.Sockets;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Services.Third_Party_Services
{
    public class EmailSender : IEmailSender<UserModel>
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendMessage(UserModel user, string messageHeader, string message)
        {
            try
            {
                string Email = _configuration[App.appEmail]!;
                string Password = _configuration[App.appEmailPassword]!;

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("FileCrypt", Email));
                emailMessage.To.Add(new MailboxAddress(user.username, user.email));
                emailMessage.Subject = messageHeader;
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
                {
                    Text = message
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
