using application.Helpers;
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
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables()
                .Build();

            string? jwtKey = configuration[App.SECRET_KEY];
            string? emailPassword = configuration[App.EMAIL_PASSWORD];
            string? emailAdress = configuration[App.EMAIL];
            string? appKey = configuration[App.ENCRYPTION_KEY];
            string? redisServer = configuration.GetConnectionString(App.REDIS_DB);
            string? postgres = configuration.GetConnectionString(App.MAIN_DB);
            string? elastic = configuration.GetConnectionString(App.ELASTIC_SEARCH);
            string? clamServer = configuration[App.CLAM_SERVER];
            string? clamPort = configuration[App.CLAM_PORT];

            bool invalidConfiguration = false;

            invalidConfiguration =
                 string.IsNullOrWhiteSpace(jwtKey) || string.IsNullOrWhiteSpace(emailPassword) ||
                 string.IsNullOrWhiteSpace(redisServer) || string.IsNullOrWhiteSpace(postgres) ||
                 string.IsNullOrWhiteSpace(emailAdress) || string.IsNullOrWhiteSpace(appKey) ||
                 string.IsNullOrWhiteSpace(elastic) || string.IsNullOrWhiteSpace(clamServer) ||
                 !int.TryParse(clamPort, out int number);

            Console.WriteLine(
                $"Email Password is valid ?: {!string.IsNullOrWhiteSpace(emailPassword)}\nEmail Password Value: {emailPassword}\n\n" +
                $"JWT Key is valid ?: {!string.IsNullOrWhiteSpace(jwtKey)}\nJwt Key Value: {jwtKey}\n\n" +
                $"Redis is valid ?: {!string.IsNullOrWhiteSpace(redisServer)}\nRedis Value: {redisServer}\n\n" +
                $"PostgreSQL is valid ?: {!string.IsNullOrWhiteSpace(postgres)}\nPostgreSQL Value: {postgres}\n\n" +
                $"Elasticsearch is valid ?: {!string.IsNullOrWhiteSpace(elastic)}\nElasticsearch Value: {elastic}\n\n" +
                $"ClamAV ( !!! ONLY CONNECTION STRING !!! ) is valid ?: {!string.IsNullOrWhiteSpace(clamServer) && !string.IsNullOrWhiteSpace(clamPort)}\nClamAV Value: {clamServer}:{clamPort}\n");

            if (invalidConfiguration)
                throw new InvalidConfigurationException();
            else
                Console.WriteLine("Configuration is valid\n");
        }
    }
}
