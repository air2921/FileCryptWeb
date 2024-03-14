using webapi.DB.RedisDb;
using webapi.Interfaces.Controllers;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Controllers.Base;
using webapi.Controllers.Base.CryptographyUtils;
using webapi.Interfaces;
using webapi.DB;
using webapi.Cryptography;
using webapi.Security;
using webapi.Helpers;
using webapi.Third_Party_Services;
using static webapi.Third_Party_Services.EmailSender;
using webapi.Controllers.Account.Edit;
using webapi.Controllers.Account;
using webapi.Controllers.Admin;

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
            services.AddScoped<ISorting, Sorting>();

            services.AddScoped<ICryptographyControllerBase, CryptographyHelper>();
            services.AddScoped<ICryptographyParamsProvider, CryptographyHelper>();
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
            services.AddScoped<IApi2FaService, _2FaService>();
            services.AddScoped<IApiEmailService, EmailService>();
            services.AddScoped<IApiPasswordService, PasswordService>();
            services.AddScoped<IApiUsernameService, UsernameService>();
            services.AddScoped<IApiRegistrationService, RegistrationService>();
            services.AddScoped<IApiSessionService, SessionService>();
            services.AddScoped<IApiRecoveryService, RecoveryService>();
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
            services.AddTransient<ICypherKey, EncryptKey>();
            services.AddTransient<ICypherKey, DecryptKey>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<ISmtpClient, SmtpClientWrapper>();
        }
    }
}
