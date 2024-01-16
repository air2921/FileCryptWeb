﻿using nClam;
using System.Text.RegularExpressions;
using webapi.Exceptions;
using webapi.Services;

namespace webapi
{
    public class AppConfigurationCheck
    {
        public static void ConfigurationCheck()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddUserSecrets<AppConfigurationCheck>()
                .Build();

            string keyPattern = Validation.EncryptionKey;
            string emailPattern = Validation.Email;

            string? secretKey = configuration[App.appSecretKey];
            string? emailPassword = configuration[App.appEmailPassword];
            string? emailAdress = configuration[App.appEmail];
            string? FileCryptKey = configuration[App.appKey];
            string? redisServer = configuration.GetConnectionString(App.RedisDb);
            string? postgres = configuration.GetConnectionString(App.MainDb);

            bool InvalidConfiguration =
                 string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(emailPassword) ||
                 string.IsNullOrWhiteSpace(redisServer) || string.IsNullOrWhiteSpace(postgres) ||
                 string.IsNullOrWhiteSpace(emailAdress) || string.IsNullOrWhiteSpace(FileCryptKey) ||
                 !Regex.IsMatch(emailAdress, emailPattern) || !Regex.IsMatch(FileCryptKey, keyPattern);

            Task.Run(async () =>
            {
                var clam = new ClamClient("localhost", 3310);
                bool isConnected = await clam.TryPingAsync();

                if (!isConnected || InvalidConfiguration)
                {
                    Console.WriteLine("Invalid Configuration:\n\n" +
                    $"ClamAV Server responsed to server ping ?: {isConnected}\n\n" +
                    $"Secret Key is valid ?: {!string.IsNullOrWhiteSpace(secretKey)}\nSecretKey Value: {secretKey}\n\n" +
                    $"Email Password is valid ?: {!string.IsNullOrWhiteSpace(emailPassword)}\nEmail Password Value: {emailPassword}\n\n" +
                    $"Email Address is valid ?: {!string.IsNullOrWhiteSpace(emailAdress) && Regex.IsMatch(emailAdress, emailPattern)}\nEmail Address Value: {emailAdress}\n\n" +
                    $"FileCrypt Key is valid ?: {!string.IsNullOrWhiteSpace(FileCryptKey) && Regex.IsMatch(FileCryptKey, keyPattern)}\nFileCrypt Key Value: {FileCryptKey}\n\n" +
                    $"Redis Server is valid ?: {!string.IsNullOrWhiteSpace(redisServer)}\nRedis Server Value: {redisServer}\n\n" +
                    $"PostgreSQL Connection String is valid ?: {!string.IsNullOrWhiteSpace(postgres)}\nPostgreSQL Connection String Value: {postgres}\n");

                    throw new InvalidConfigurationException();
                }
            }).GetAwaiter().GetResult();

            Console.WriteLine("Configuration is valid\n");
        }
    }
}
