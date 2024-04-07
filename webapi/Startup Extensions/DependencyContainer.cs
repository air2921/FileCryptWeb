using webapi.Cryptography;
using webapi.Cryptography.Abstractions;
using webapi.DB.Abstractions;
using webapi.DB.Ef;
using webapi.DB.RedisDb;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Helpers.Security;
using webapi.Models;
using webapi.Services.Abstractions;
using webapi.Services.Account;
using webapi.Services.Admin;
using webapi.Services.Core;
using webapi.Services.Core.Data_Handlers;
using webapi.Third_Party_Services;
using webapi.Third_Party_Services.Abstractions;
using static webapi.Third_Party_Services.EmailSender;

namespace webapi
{
    public class DependencyContainer
    {
        public static void Singleton(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        public static void Scoped(IServiceCollection services)
        {
            services.AddScoped<IRedisKeys, RedisKeys>();
            services.AddScoped<IRedisCache, RedisCache>();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IDatabaseTransaction, DatabaseTransaction>();

            services.AddScoped<ICryptographyHelper, CryptographyHelper>();
            services.AddScoped<ICryptographyProvider, CryptographyService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IValidation, Validation>();
            services.AddScoped<IGenerate, Generate>();
            services.AddScoped<IGetSize, FileManager>();
            services.AddScoped<IFileManager, FileManager>();
            services.AddScoped<IPasswordManager, PasswordManager>();
            services.AddScoped<IVirusCheck, ClamAV>();
            services.AddScoped<IClamSetting, ClamSetting>();
            services.AddScoped<IUserInfo, UserData>();
            services.AddScoped<IAes, AesCreator>();
        }

        public static void OtherServices(IServiceCollection services)
        {
            HelperServices(services);
            CoreKeyedServices(services);
            AccountKeyedServices(services);
            AdminKeyedServices(services);
            CacheServices(services);
        }

        private static void HelperServices(IServiceCollection services)
        {
            services.AddScoped<IUserHelpers, UserService>();
            services.AddTransient<IRecoveryHelpers, RecoveryService>();
            services.AddScoped<ISessionHelpers, SessionService>();
            services.AddScoped<IKeyHelper, KeyService>();
        }

        private static void CoreKeyedServices(IServiceCollection services)
        {
            services.AddKeyedTransient<ITransaction<KeyModel>, OfferService>(ImplementationKey.CORE_OFFER_SERVICE);
            services.AddKeyedTransient<ITransaction<Participants>, OfferService>(ImplementationKey.CORE_OFFER_SERVICE);
            services.AddKeyedScoped<IDataManagement, OfferService>(ImplementationKey.CORE_OFFER_SERVICE);

            services.AddKeyedScoped<IValidator, KeyStorageService>(ImplementationKey.CORE_KEY_STORAGE_SERVICE);
            services.AddKeyedScoped<IValidator, KeyService>(ImplementationKey.CORE_KEY_SERVICE);
            services.AddKeyedScoped<IDataManagement, KeyService>(ImplementationKey.CORE_KEY_SERVICE);
        }

        private static void AdminKeyedServices(IServiceCollection services)
        {
            services.AddKeyedScoped<ITransaction<TokenModel>, AdminTokenService>(ImplementationKey.ADMIN_TOKEN_SERVICE);
            services.AddKeyedScoped<IValidator, AdminTokenService>(ImplementationKey.ADMIN_TOKEN_SERVICE);

            services.AddKeyedScoped<ITransaction<UserModel>, AdminUserService>(ImplementationKey.ADMIN_USER_SERVICE);
            services.AddKeyedScoped<IValidator, AdminUserService>(ImplementationKey.ADMIN_USER_SERVICE);
        }

        private static void AccountKeyedServices(IServiceCollection services)
        {
            services.AddKeyedScoped<ITransaction<UserModel>, _2FaService>(ImplementationKey.ACCOUNT_2FA_SERVICE);
            services.AddKeyedScoped<IDataManagement, _2FaService>(ImplementationKey.ACCOUNT_2FA_SERVICE);
            services.AddKeyedScoped<IValidator, _2FaService>(ImplementationKey.ACCOUNT_2FA_SERVICE);

            services.AddKeyedScoped<ITransaction<UserModel>, EmailService>(ImplementationKey.ACCOUNT_EMAIL_SERVICE);
            services.AddKeyedScoped<IDataManagement, EmailService>(ImplementationKey.ACCOUNT_EMAIL_SERVICE);
            services.AddKeyedScoped<IValidator, EmailService>(ImplementationKey.ACCOUNT_EMAIL_SERVICE);

            services.AddKeyedScoped<ITransaction<UserModel>, PasswordService>(ImplementationKey.ACCOUNT_PASSWORD_SERVICE);
            services.AddKeyedScoped<IDataManagement, PasswordService>(ImplementationKey.ACCOUNT_PASSWORD_SERVICE);
            services.AddKeyedScoped<IValidator, PasswordService>(ImplementationKey.ACCOUNT_PASSWORD_SERVICE);

            services.AddKeyedScoped<ITransaction<UserModel>, UsernameService>(ImplementationKey.ACCOUNT_USERNAME_SERVICE);
            services.AddKeyedScoped<IDataManagement, UsernameService>(ImplementationKey.ACCOUNT_USERNAME_SERVICE);
            services.AddKeyedScoped<IValidator, UsernameService>(ImplementationKey.ACCOUNT_USERNAME_SERVICE);

            services.AddKeyedScoped<ITransaction<User>, RegistrationService>(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE);
            services.AddKeyedScoped<IDataManagement, RegistrationService>(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE);
            services.AddKeyedScoped<IValidator, RegistrationService>(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE);

            services.AddKeyedScoped<IDataManagement, SessionService>(ImplementationKey.ACCOUNT_SESSION_SERVICE);
            services.AddKeyedScoped<IValidator, RecoveryService>(ImplementationKey.ACCOUNT_RECOVERY_SERVICE);
        }

        private static void CacheServices(IServiceCollection services)
        {
            services.AddScoped<ICacheHandler<FileModel>, Files>();
            services.AddScoped<ICacheHandler<NotificationModel>, Notifications>();
            services.AddScoped<ICacheHandler<OfferModel>, Offers>();
            services.AddScoped<ICacheHandler<UserModel>, Users>();
            services.AddScoped<ICacheHandler<KeyModel>, Keys>();
        }

        public static void Transient(IServiceCollection services)
        {
            services.AddTransient<IRedisDbContext, RedisDbContext>();
            services.AddTransient<ICypher, Cypher>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddKeyedTransient<ICypherKey, EncryptKey>("Encrypt");
            services.AddKeyedTransient<ICypherKey, DecryptKey>("Decrypt");
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<ISmtpClient, SmtpClientWrapper>();
        }
    }
}
