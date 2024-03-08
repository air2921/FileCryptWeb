using nClam;
using System.Text.RegularExpressions;
using webapi.Exceptions;
using webapi.Helpers;

namespace webapi
{
    public class AppConfigurationCheck
    {
        /// <summary>
        /// Using GetAwaiter().GetResult() or .Result is bad practice and may cause a deadlock
        /// </summary>
        /// <exception cref="InvalidConfigurationException"></exception>

        public static void ConfigurationCheck()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddUserSecrets<AppConfigurationCheck>()
                .Build();

            string keyPattern = Validation.EncryptionKey;
            string emailPattern = Validation.Email;

            string? secretKey = configuration[App.SECRET_KEY];
            string? emailPassword = configuration[App.EMAIL_PASSWORD];
            string? emailAdress = configuration[App.EMAIL];
            string? FileCryptKey = configuration[App.ENCRYPTION_KEY];
            string? redisServer = configuration.GetConnectionString(App.REDIS_DB);
            string? postgres = configuration.GetConnectionString(App.MAIN_DB);

            bool InvalidConfiguration =
                 string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(emailPassword) ||
                 string.IsNullOrWhiteSpace(redisServer) || string.IsNullOrWhiteSpace(postgres) ||
                 string.IsNullOrWhiteSpace(emailAdress) || string.IsNullOrWhiteSpace(FileCryptKey) ||
                 !Regex.IsMatch(emailAdress, emailPattern) || !Regex.IsMatch(FileCryptKey, keyPattern);

#if DEBUG

            Task.Run(async () =>
            {
                var clam = new ClamClient("localhost", 3310);
                bool isConnected = await clam.TryPingAsync();

                if (!isConnected || InvalidConfiguration)
                {
                    Console.WriteLine("Invalid Configuration:\n\n" +
                        $"Secret Key is valid ?: {!string.IsNullOrWhiteSpace(secretKey)}\nSecretKey Value: {secretKey}\n\n" +
                        $"Email Password is valid ?: {!string.IsNullOrWhiteSpace(emailPassword)}\nEmail Password Value: {emailPassword}\n\n" +
                        $"Email Address is valid ?: {!string.IsNullOrWhiteSpace(emailAdress) && Regex.IsMatch(emailAdress, emailPattern)}\nEmail Address Value: {emailAdress}\n\n" +
                        $"FileCrypt Key is valid ?: {!string.IsNullOrWhiteSpace(FileCryptKey) && Regex.IsMatch(FileCryptKey, keyPattern)}\nFileCrypt Key Value: {FileCryptKey}\n\n" +
                        $"Redis Server is valid ?: {!string.IsNullOrWhiteSpace(redisServer)}\nRedis Server Value: {redisServer}\n\n" +
                        $"PostgreSQL Connection String is valid ?: {!string.IsNullOrWhiteSpace(postgres)}\nPostgreSQL Connection String Value: {postgres}\n");

                    throw new InvalidConfigurationException();
                }

            }).GetAwaiter().GetResult();

#endif

            Console.WriteLine("Configuration is valid\n");
        }
    }
}
