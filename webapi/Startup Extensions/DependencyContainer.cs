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
using webapi.Helpers.DataManager;
using static webapi.Third_Party_Services.EmailSender;

namespace webapi
{
    public class DependencyContainer
    {
        public static void Singleton(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IRedisDbContext, RedisDbContext>();
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
            services.AddScoped<IGenerateSixDigitCode, GenerateCodesAndKeys>();
            services.AddScoped<IGenerateKey, GenerateCodesAndKeys>();
            services.AddScoped<IPasswordManager, PasswordManager>();
            services.AddScoped<IVirusCheck, ClamAV>();
            services.AddScoped<IClamSetting, ClamSetting>();
            services.AddScoped<IUserInfo, UserData>();
            services.AddScoped<IUserAgent, UserAgent>();
            services.AddScoped<IAes, AesCreator>();
        }

        public static void Transient(IServiceCollection services)
        {
            services.AddTransient<IGetSize, GetSize>();
            services.AddTransient<ICypher, Cypher>();
            services.AddTransient<IFileManager, FileManager>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IEncryptKey, EncryptKey>();
            services.AddTransient<IDecryptKey, EncryptKey>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<ISmtpClient, SmtpClientWrapper>();
        }
    }
}
