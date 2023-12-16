using nClam;
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
                    throw new InvalidConfigurationException();
            }).GetAwaiter().GetResult();

            Console.WriteLine("Configuration is valid\n");
        }
    }
}
