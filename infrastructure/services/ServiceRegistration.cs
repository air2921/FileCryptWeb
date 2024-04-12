using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using services.Abstractions;
using services.ClamAv;
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

            services.AddScoped<IVirusCheck, ClamAV>();
            services.AddScoped<IClamSetting, ClamSetting>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<ISmtpClient, SmtpClientWrapper>();
        }
    }
}
