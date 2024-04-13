using domain.Abstractions;
using domain.Abstractions.Services;
using domain.DTO;
using domain.Helpers;
using domain.Services.Abstractions;
using domain.Services.Additional;
using domain.Services.Master_Services.Account;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace domain
{
    public static class ServiceRegistration
    {
        public static void AddDomain(this IServiceCollection services, IConfiguration _config)
        {
            services.Configure<Secret>(options =>
            {
                options.Key = Convert.FromBase64String(_config[App.ENCRYPTION_KEY]!);
            });

            services.AddScoped<Secret>();
            services.AddScoped<IValidation, Validation>();

            services.AddScoped<IRegistrationService, RegistrationService>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<IRecoveryService, RecoveryService>();

            services.AddKeyedScoped<ITransaction<UserDTO>, RegistrationHelper>(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE);
            services.AddKeyedScoped<IDataManagement, RegistrationHelper>(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE);
            services.AddKeyedScoped<IValidator, RegistrationHelper>(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE);

            services.AddKeyedScoped<IDataManagement, SessionHelper>(ImplementationKey.ACCOUNT_SESSION_SERVICE);
            services.AddKeyedScoped<IValidator, RecoveryHelper>(ImplementationKey.ACCOUNT_RECOVERY_SERVICE);
        }
    }
}
