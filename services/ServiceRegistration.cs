using application.Abstractions.TP_Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using services.ClamAv;
using services.Cryptography;
using services.Cryptography.Abstractions;
using services.Helpers;
using services.Helpers.Security;
using services.Sender;
using static services.Sender.EmailSender;

namespace services
{
    public static class ServiceRegistration
    {
        public static void AddServicesInfrastructure(this IServiceCollection services, IConfiguration _config)
        {
            services.Configure<EmailSender>(options =>
            {
                options.Email = _config["Email"]!;
            });

            services.Configure<SmtpClientWrapper>(options =>
            {
                options.Email = _config["Email"]!;
                options.Password = _config["EmailPassword"]!;
            });

            services.Configure<ClamAV>(options =>
            {
                options.ClamServer = _config["ClamServer"]!;
                options.ClamPort = int.Parse(_config["ClamPort"]!);
            });

            services.Configure<FileManager>(options =>
            {
                options.ReactConnection = _config["ReactDomain"]!;
            });

            services.AddLogging();

            services.AddScoped<IAes, AesCreator>();
            services.AddScoped<IVirusCheck, ClamAV>();
            services.AddScoped<IClamSetting, ClamSetting>();
            services.AddScoped<IGenerate, Generate>();
            services.AddScoped<IHashUtility, HashUtility>();
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
