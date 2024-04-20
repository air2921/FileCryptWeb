﻿using application.Abstractions.Services.Endpoints;
using application.Abstractions.Services.Inner;
using application.DTO.Inner;
using application.Helpers;
using application.Services.Abstractions;
using application.Services.Additional.Account;
using application.Services.Additional.Account.Edit;
using application.Services.Additional.Admin;
using application.Services.Additional.Core;
using application.Services.Cache_Handlers;
using application.Services.Master_Services.Account;
using application.Services.Master_Services.Account.Edit;
using domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace application
{
    public static class ServiceRegistration
    {
        public static void AddDomain(this IServiceCollection services, IConfiguration _config)
        {
            services.AddScoped<IValidation, Validation>();
            services.AddScoped<ICryptographyHelper, CryptographyHelper>();
            services.AddScoped<IFileHelper, FileHelper>();

            services.AddUpperModuleServices();
            services.AddCacheServices();
            services.AddAccountKeyedServices();
            services.AddAdminKeyedServices();
            services.AddCoreServices();
        }

        private static void AddAccountKeyedServices(this IServiceCollection services)
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

        private static void AddAdminKeyedServices(this IServiceCollection services)
        {
            services.AddKeyedScoped<ITransaction<TokenModel>, TokenService>(ImplementationKey.ADMIN_TOKEN_SERVICE);
            services.AddKeyedScoped<IValidator, TokenService>(ImplementationKey.ADMIN_TOKEN_SERVICE);

            services.AddKeyedScoped<ITransaction<UserModel>, UserService>(ImplementationKey.ADMIN_USER_SERVICE);
            services.AddKeyedScoped<IValidator, UserService>(ImplementationKey.ADMIN_USER_SERVICE);
        }

        private static void AddCoreServices(this IServiceCollection services)
        {
            services.AddScoped<ICryptographyHelper, CryptographyHelper>();
            services.AddScoped<IFileHelper, FileHelper>();
            services.AddScoped<ITransaction<CreateOfferDTO>, OfferHelper>();
            services.AddScoped<ITransaction<AcceptOfferDTO>, OfferHelper>();
            services.AddKeyedScoped<IDataManagement, OfferHelper>(ImplementationKey.CORE_OFFER_SERVICE);
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
}