using application.Abstractions.TP_Services;
using application.Master_Services.Account.Edit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using services.ClamAv;
using services.Cryptography;
using services.Cryptography.Abstractions;
using services.Cryptography.Helpers;
using services.Cryptography.Helpers.Security;
using services.Sender;
using static services.Sender.EmailSender;

namespace services
{
    public static class ServiceRegistration
    {
        public static void AddServiceInfrastructure(this IServiceCollection services,
            IConfiguration _config, Serilog.ILogger logger)
        {
            services.AddLogging(log =>
            {
                log.ClearProviders();
                log.AddSerilog(logger);
            });

            services.AddScoped<IAes, AesCreator>();
            services.AddScoped<IClamSetting, ClamSetting>();
            services.AddScoped<IGenerate, Generate>();
            services.AddScoped<IHashUtility, HashUtility>();
            services.AddScoped<IGetSize, FileManager>();

            services.AddTransient<ICypher, Cypher>();
            services.AddKeyedTransient<ICypherKey, EncryptKey>("Encrypt");
            services.AddKeyedTransient<ICypherKey, DecryptKey>("Decrypt");

            services.AddScoped<ISmtpClient>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<SmtpClientWrapper>>();
                return new SmtpClientWrapper(logger, _config);
            });

            services.AddScoped<IEmailSender>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<EmailSender>>();
                var smtpClient = provider.GetRequiredService<ISmtpClient>();
                return new EmailSender(_config, logger, smtpClient);
            });

            services.AddScoped<IFileManager>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<FileManager>>();
                return new FileManager(logger, _config);
            });

            services.AddScoped<IVirusCheck>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<ClamAV>>();
                var clamSettings = provider.GetRequiredService<IClamSetting>();
                return new ClamAV(logger, _config, clamSettings);
            });
        }
    }
}
