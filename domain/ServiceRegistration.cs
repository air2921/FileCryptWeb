using domain.Abstractions;
using domain.DTO;
using domain.Helpers;
using domain.Models;
using domain.Services;
using domain.Services.Abstractions;
using domain.Services.Additional.Account;
using domain.Services.Master_Services.Account;
using domain.Services.Master_Services.Account.Edit;
using domain.Upper_Module.Services;
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
            services.AddScoped<IUsernameService, UsernameService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<I2FaService, _2FaService>();

            services.AddScoped<ICacheHandler<FileModel>, Files>();
            services.AddScoped<ICacheHandler<KeyModel>, Keys>();
            services.AddScoped<ICacheHandler<NotificationModel>, Notifications>();
            services.AddScoped<ICacheHandler<OfferModel>, Offers>();
            services.AddScoped<ICacheHandler<UserModel>, Users>();

            services.AddKeyedScoped<ITransaction<UserDTO>, RegistrationHelper>(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE);
            services.AddKeyedScoped<IDataManagement, RegistrationHelper>(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE);
            services.AddKeyedScoped<IValidator, RegistrationHelper>(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE);

            services.AddKeyedScoped<IDataManagement, SessionHelper>(ImplementationKey.ACCOUNT_SESSION_SERVICE);
            services.AddKeyedScoped<IValidator, RecoveryHelper>(ImplementationKey.ACCOUNT_RECOVERY_SERVICE);
        }
    }
}
