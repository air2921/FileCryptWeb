using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using services.Abstractions;
using services.ClamAv;
using services.Cryptography;
using services.Cryptography.Abstractions;
using services.Helpers;
using services.Helpers.Security;
using services.Sender;
using shared.Immutable;
using static services.Sender.EmailSender;

namespace services
{
    public static class ServiceRegistration
    {
        public static void AddServicesInfrastructure(this IServiceCollection services, IConfiguration _config)
        {
            services.Configure<EmailSender>(options =>
            {
                options.Email = _config[App.EMAIL]!;
            });

            services.Configure<SmtpClientWrapper>(options =>
            {
                options.Email = _config[App.EMAIL]!;
                options.Password = _config[App.EMAIL_PASSWORD]!;
            });

            services.Configure<ClamAV>(options =>
            {
                options.ClamServer = _config[App.CLAM_SERVER]!;
                options.ClamPort = int.Parse(_config[App.CLAM_PORT]!);
            });

            services.Configure<FileManager>(options =>
            {
                options.ReactConnection = _config[App.REACT_APP]!;
            });

            services.AddScoped<IVirusCheck, ClamAV>();
            services.AddScoped<IClamSetting, ClamSetting>();
            services.AddScoped<IGenerate, Generate>();
            services.AddScoped<IPasswordManager, PasswordManager>();
            services.AddScoped<IFileManager, FileManager>();
            services.AddScoped<IGetSize, FileManager>();

            services.AddTransient<ICypher, Cypher>();
            services.AddKeyedTransient<ICypherKey, EncryptKey>("Encrypt");
            services.AddKeyedTransient<ICypherKey, DecryptKey>("Decrypt");
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<ISmtpClient, SmtpClientWrapper>();
        }
    }
}
