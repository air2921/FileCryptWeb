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
        }
    }
}
