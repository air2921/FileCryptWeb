using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using nClam;
using System.Text;
using System.Text.RegularExpressions;

using webapi.Controllers.Base;
using webapi.Controllers.Base.CryptographyUtils;
using webapi.Services.Cryptography;
using webapi.Services.DataManager;
using webapi.Services.Security;
using webapi.Services.Third_Party_Services;
using webapi.DB;
using webapi.DB.MongoDb;
using webapi.DB.RedisDb;
using webapi.DB.SQL.API;
using webapi.DB.SQL.Files;
using webapi.DB.SQL.Files.Mimes;
using webapi.DB.SQL.Keys;
using webapi.DB.SQL.Notifications;
using webapi.DB.SQL.Offers;
using webapi.DB.SQL.Tokens;
using webapi.DB.SQL.Users;
using webapi.Interfaces.Controllers;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Mongo;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Interfaces.SQL.API;
using webapi.Interfaces.SQL.Files;
using webapi.Interfaces.SQL.Files.Mimes;
using webapi.Interfaces.SQL.Keys;
using webapi.Interfaces.SQL.Notifications;
using webapi.Interfaces.SQL.Offers;
using webapi.Interfaces.SQL.Tokens;
using webapi.Interfaces.SQL.User;
using webapi.Middlewares;
using webapi.Exceptions;
using webapi.Models;
using webapi.Services;

namespace webapi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            string keyPattern = Validation.EncryptionKey;
            string emailPattern = Validation.Email;

            string? secretKey = configuration["SecretKey"];
            string? emailPassword = configuration["EmailPassword"];
            string? emailAdress = configuration["Email"];
            string? FileCryptKey = configuration["FileCryptKey"];
            string? redisServer = configuration.GetConnectionString("RedisConnection");
            string? mongoDb = configuration.GetConnectionString("MongoDbConnection");
            string? postgres = configuration.GetConnectionString("PostgresConnection");

            bool InvalidConfiguration =
                 string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(emailPassword) ||
                 string.IsNullOrWhiteSpace(redisServer) || string.IsNullOrWhiteSpace(mongoDb) || string.IsNullOrWhiteSpace(postgres) ||
                 string.IsNullOrWhiteSpace(emailAdress) || string.IsNullOrWhiteSpace(FileCryptKey) ||
                 !Regex.IsMatch(emailAdress, emailPattern) || !Regex.IsMatch(FileCryptKey, keyPattern);

            Task.Run(async () =>
            {
                var clam = new ClamClient("localhost", 3310);
                bool isConnected = await clam.TryPingAsync();

                if (!isConnected || InvalidConfiguration)
                    throw new InvalidConfigurationException();
            }).GetAwaiter().GetResult();

            services.AddDbContext<FileCryptDbContext>(options => options.UseNpgsql(postgres));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAes, AesCreator>();
            services.AddSingleton<IValidation, Validation>();
            services.AddSingleton<IRedisDbContext, RedisDbContext>();
            services.AddSingleton<IMongoDbContext, MongoDbContext>();
            services.AddSingleton<IPasswordManager, PasswordManager>();
            services.AddSingleton<IGenerateKey, GenerateCodesAndKeys>();
            services.AddSingleton<IGenerateSixDigitCode, GenerateCodesAndKeys>();

            services.AddScoped<IUserInfo, UserData>();
            services.AddScoped<IRedisKeys, RedisKeys>();
            services.AddScoped<IRedisCache, RedisCache>();

            services.AddScoped<ICreate<UserModel>, CreateUser>();
            services.AddScoped<IDelete<UserModel>, DeleteUser>();
            services.AddScoped<IReadUser, ReadUser>();
            services.AddScoped<IUpdateUser, UpdateUser>();

            services.AddScoped<ICreate<KeyModel>, CreateKey>();
            services.AddScoped<IReadKeys, ReadKeys>();
            services.AddScoped<IUpdateKeys, UpdateKeys>();

            services.AddScoped<ICreate<FileModel>, CreateFile>();
            services.AddScoped<IDeleteFile, DeleteFile>();
            services.AddScoped<IReadFile, ReadFile>();

            services.AddScoped<ICreate<NotificationModel>, CreateNotification>();
            services.AddScoped<IDelete<NotificationModel>, DeleteNotification>();
            services.AddScoped<IReadNotifications, ReadNotifications>();

            services.AddScoped<ICreate<OfferModel>, CreateOffer>();
            services.AddScoped<IDelete<OfferModel>, DeleteOffer>();
            services.AddScoped<IReadOffer, ReadOffer>();
            services.AddScoped<IUpdateOffer, UpdateOffer>();

            services.AddScoped<ICreate<FileMimeModel>, CreateMime>();
            services.AddScoped<IInsertBase<FileMimeModel>, CreateMime>();
            services.AddScoped<IDelete<FileMimeModel>, DeleteMime>();
            services.AddScoped<IDeleteByName<FileMimeModel>, DeleteMime>();
            services.AddScoped<IReadMime, ReadMime>();

            services.AddScoped<ICreate<TokenModel>, CreateToken>();
            services.AddScoped<IReadToken, ReadToken>();
            services.AddScoped<IUpdateToken, UpdateToken>();

            services.AddScoped<ICreate<ApiModel>, CreateAPI>();
            services.AddScoped<IDelete<ApiModel>, DeleteAPI>();
            services.AddScoped<IDeleteByName<ApiModel>, DeleteAPI>();
            services.AddScoped<IReadAPI, ReadAPI>();
            services.AddScoped<IUpdateAPI, UpdateAPI>();

            services.AddScoped<ICryptographyControllerBase, CryptographyControllerBase>();
            services.AddScoped<ICryptographyParamsProvider, CryptographyParamsProvider>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IVirusCheck, ClamAV>();

            services.AddTransient<IGetSize, GetSize>();
            services.AddTransient<IEncrypt, EncryptAsync>();
            services.AddTransient<IDecrypt, DecryptAsync>();
            services.AddTransient<IFileManager, FileManager>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IEncryptKey, EncryptKey>();
            services.AddTransient<IDecryptKey, EncryptKey>();
            services.AddTransient<IEmailSender<UserModel>, EmailSender>();

            services.AddControllers();
            services.AddLogging();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddDistributedMemoryCache();
            services.AddAuthorization();

            services.AddSession(session =>
            {
                session.IOTimeout = TimeSpan.FromMinutes(15);
                session.Cookie.HttpOnly = true;
                session.Cookie.SameSite = SameSiteMode.Strict;
                session.Cookie.IsEssential = true;
                session.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwt =>
            {
                jwt.RequireHttpsMetadata = true;
                jwt.SaveToken = true;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["SecretKey"]!)),
                    ValidIssuer = "FileCrypt",
                    ValidAudience = "User",
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 75 * 1024 * 1024;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseFreeze();
            app.UseAPI();
            app.UseSession();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseBearer();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseLog();

            app.UseEndpoints(endpoint =>
            {
                endpoint.MapControllers();
            });
        }
    }
}
