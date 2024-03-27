using webapi.DB.RedisDb;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces;
using webapi.DB;
using webapi.Cryptography;
using webapi.Security;
using webapi.Helpers;
using webapi.Third_Party_Services;
using static webapi.Third_Party_Services.EmailSender;
using webapi.Controllers.Admin;
using webapi.Interfaces.Controllers.Services;
using webapi.Models;
using webapi.Services.Account;
using webapi.Services.Core;

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
            services.AddScoped<ISorting, Sorting>();

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

            ControllerServices(services);
        }

        private static void ControllerServices(IServiceCollection services)
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

            services.AddKeyedScoped<ITransaction<UserObject>, RegistrationService>(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE);
            services.AddKeyedScoped<IDataManagement, RegistrationService>(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE);
            services.AddKeyedScoped<IValidator, RegistrationService>(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE);

            services.AddKeyedScoped<IDataManagement, SessionService>(ImplementationKey.ACCOUNT_SESSION_SERVICE);
            services.AddScoped<ISessionHelpers, SessionService>();

            services.AddKeyedScoped<IValidator, RecoveryService>(ImplementationKey.ACCOUNT_RECOVERY_SERVICE);
            services.AddTransient<IRecoveryHelpers, RecoveryService>();

            services.AddKeyedTransient<ITransaction<KeyModel>, OfferService>(ImplementationKey.CORE_OFFER_SERVICE);
            services.AddKeyedTransient<ITransaction<Participants>, OfferService>(ImplementationKey.CORE_OFFER_SERVICE);
            services.AddKeyedScoped<IDataManagement, OfferService>(ImplementationKey.CORE_OFFER_SERVICE);

            services.AddKeyedScoped<IValidator, KeyStorageService>(ImplementationKey.CORE_KEY_STORAGE_SERVICE);
            services.AddScoped<IStorageHelpers, KeyStorageService>();

            services.AddScoped<IUserHelpers, UserService>();

            services.AddScoped<IApiAdminKeysService, AdminKeysService>();
            services.AddScoped<IApiAdminTokenService, AdminTokenService>();
            services.AddScoped<IApiAdminUserService, AdminUserService>();
        }

        public static void Transient(IServiceCollection services)
        {
            services.AddTransient<IImplementationFinder, ImplementationFinder>();
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
