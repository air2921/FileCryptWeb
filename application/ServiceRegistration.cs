using application.Abstractions.Services.Endpoints;
using application.Abstractions.Services.Inner;
using application.Cache_Handlers;
using application.DTO.Inner;
using application.Helper_Services;
using application.Helper_Services.Account;
using application.Helper_Services.Account.Edit;
using application.Helper_Services.Admin;
using application.Helper_Services.Core;
using application.Helpers;
using application.Master_Services.Account;
using application.Master_Services.Account.Edit;
using domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace application
{
    public static class ServiceRegistration
    {
        public static void AddDomain(this IServiceCollection services, IConfiguration _config)
        {
            services.Configure<TokenComparator>(options =>
            {
                options.Key = _config[App.SECRET_KEY]!;
            });

            services.AddScoped<ITokenComparator, TokenComparator>();

            services.AddUpperModuleServices();
            services.AddCacheServices();

            services.AddAccountKeyedServices();
            services.AddAdminKeyedServices();
            services.AddCoreServices();
        }

        private static void AddCacheServices(this IServiceCollection services)
        {
            services.AddScoped<ICacheHandler<FileModel>, Files>();
            services.AddScoped<ICacheHandler<NotificationModel>, Notifications>();
            services.AddScoped<ICacheHandler<OfferModel>, Offers>();
            services.AddScoped<ICacheHandler<UserModel>, Users>();
            services.AddScoped<ICacheHandler<KeyStorageItemModel>, StorageItems>();
            services.AddScoped<ICacheHandler<KeyStorageModel>, Storages>();
        }

        private static void AddUpperModuleServices(this IServiceCollection services)
        {
            services.AddScoped<IRegistrationService, RegistrationService>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<IRecoveryService, RecoveryService>();
            services.AddScoped<IUsernameService, UsernameService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<I2FaService, _2FaService>();
            services.AddScoped<IEmailService, EmailService>();
        }
    }





    internal static class RegisterHelpers
    {
        internal static void AddAccountKeyedServices(this IServiceCollection services)
        {
            services.AddKeyedScoped<ITransaction<UserModel>, _2FaHelper>(ImplementationKey.ACCOUNT_2FA_SERVICE);
            services.AddKeyedScoped<IDataManagement, _2FaHelper>(ImplementationKey.ACCOUNT_2FA_SERVICE);
            services.AddKeyedScoped<IValidator, _2FaHelper>(ImplementationKey.ACCOUNT_2FA_SERVICE);

            services.AddKeyedScoped<ITransaction<UserModel>, EmailHelper>(ImplementationKey.ACCOUNT_EMAIL_SERVICE);
            services.AddKeyedScoped<IDataManagement, EmailHelper>(ImplementationKey.ACCOUNT_EMAIL_SERVICE);
            services.AddKeyedScoped<IValidator, EmailHelper>(ImplementationKey.ACCOUNT_EMAIL_SERVICE);

            services.AddKeyedScoped<ITransaction<UserModel>, PasswordHelper>(ImplementationKey.ACCOUNT_PASSWORD_SERVICE);
            services.AddKeyedScoped<IDataManagement, PasswordHelper>(ImplementationKey.ACCOUNT_PASSWORD_SERVICE);

            services.AddKeyedScoped<ITransaction<UserDTO>, RegistrationHelper>(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE);
            services.AddKeyedScoped<IDataManagement, RegistrationHelper>(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE);
            services.AddKeyedScoped<IValidator, RegistrationHelper>(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE);

            services.AddKeyedScoped<IDataManagement, SessionHelper>(ImplementationKey.ACCOUNT_SESSION_SERVICE);
            services.AddKeyedScoped<IValidator, RecoveryHelper>(ImplementationKey.ACCOUNT_RECOVERY_SERVICE);
        }

        internal static void AddAdminKeyedServices(this IServiceCollection services)
        {
            services.AddKeyedScoped<ITransaction<TokenModel>, TokenService>(ImplementationKey.ADMIN_TOKEN_SERVICE);
            services.AddKeyedScoped<IValidator, TokenService>(ImplementationKey.ADMIN_TOKEN_SERVICE);

            services.AddKeyedScoped<ITransaction<UserModel>, UserService>(ImplementationKey.ADMIN_USER_SERVICE);
            services.AddKeyedScoped<IValidator, UserService>(ImplementationKey.ADMIN_USER_SERVICE);
        }

        internal static void AddCoreServices(this IServiceCollection services)
        {
            services.AddScoped<ICryptographyHelper, CryptographyHelper>();
            services.AddScoped<IFileHelper, FileHelper>();
            services.AddScoped<ITransaction<CreateOfferDTO>, OfferHelper>();
            services.AddScoped<ITransaction<AcceptOfferDTO>, OfferHelper>();
            services.AddKeyedScoped<IDataManagement, OfferHelper>(ImplementationKey.CORE_OFFER_SERVICE);
        }
    }
}
