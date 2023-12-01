﻿using webapi.Controllers.Base.CryptographyUtils;
using webapi.Controllers.Base;
using webapi.DB.MongoDb;
using webapi.DB.RedisDb;
using webapi.DB.SQL;
using webapi.Interfaces.Controllers;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Mongo;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Models;
using webapi.Services;
using webapi.Services.Cryptography;
using webapi.Services.Security;
using webapi.Services.Third_Party_Services;
using webapi.Services.DataManager;

namespace webapi
{
    public class DiContainer
    {
        public static void Singleton(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAes, AesCreator>();
            services.AddSingleton<IValidation, Validation>();
            services.AddSingleton<IRedisDbContext, RedisDbContext>();
            services.AddSingleton<IMongoDbContext, MongoDbContext>();
            services.AddSingleton<IPasswordManager, PasswordManager>();
            services.AddSingleton<IGenerateKey, GenerateCodesAndKeys>();
            services.AddSingleton<IGenerateSixDigitCode, GenerateCodesAndKeys>();
        }

        public static void Scoped(IServiceCollection services)
        {
            services.AddScoped<IRedisKeys, RedisKeys>();
            services.AddScoped<IRedisCache, RedisCache>();

            services.AddScoped<ICreate<UserModel>, Users>();
            services.AddScoped<IDelete<UserModel>, Users>();
            services.AddScoped<IRead<UserModel>, Users>();
            services.AddScoped<IUpdate<UserModel>, Users>();

            services.AddScoped<ICreate<KeyModel>, Keys>();
            services.AddScoped<IRead<KeyModel>, Keys>();
            services.AddScoped<IUpdateKeys, UpdateKeys>();

            services.AddScoped<ICreate<FileModel>, Files>();
            services.AddScoped<IDelete<FileModel>, Files>();
            services.AddScoped<IRead<FileModel>, Files>();

            services.AddScoped<ICreate<NotificationModel>, Notifications>();
            services.AddScoped<IDelete<NotificationModel>, Notifications>();
            services.AddScoped<IRead<NotificationModel>, Notifications>();

            services.AddScoped<ICreate<OfferModel>, Offers>();
            services.AddScoped<IDelete<OfferModel>, Offers>();
            services.AddScoped<IRead<OfferModel>, Offers>();

            services.AddScoped<ICreate<FileMimeModel>, Mimes>();
            services.AddScoped<IInsertBase<FileMimeModel>, Mimes>();
            services.AddScoped<IDelete<FileMimeModel>, Mimes>();
            services.AddScoped<IDeleteByName<FileMimeModel>, Mimes>();
            services.AddScoped<IRead<FileMimeModel>, Mimes>();

            services.AddScoped<ICreate<TokenModel>, Tokens>();
            services.AddScoped<IRead<TokenModel>, Tokens>();
            services.AddScoped<IUpdate<TokenModel>, Tokens>();

            services.AddScoped<ICreate<ApiModel>, Api>();
            services.AddScoped<IDelete<ApiModel>, Api>();
            services.AddScoped<IDeleteByName<ApiModel>, Api>();
            services.AddScoped<IRead<ApiModel>, Api>();
            services.AddScoped<IUpdate<ApiModel>, Api>();

            services.AddScoped<ICryptographyControllerBase, CryptographyControllerBase>();
            services.AddScoped<ICryptographyParamsProvider, CryptographyParamsProvider>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IVirusCheck, ClamAV>();
            services.AddScoped<IUserInfo, UserData>();
        }

        public static void Transient(IServiceCollection services)
        {
            services.AddTransient<IGetSize, GetSize>();
            services.AddTransient<IEncrypt, EncryptAsync>();
            services.AddTransient<IDecrypt, DecryptAsync>();
            services.AddTransient<IFileManager, FileManager>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IEncryptKey, EncryptKey>();
            services.AddTransient<IDecryptKey, EncryptKey>();
            services.AddTransient<IEmailSender<UserModel>, EmailSender>();
        }
    }
}