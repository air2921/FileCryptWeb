namespace webapi.Exceptions
{
    public class InvalidConfigurationException : Exception
    {
        /// <summary>
        /// An exception is only needed for forwarding in case of invalid configuration settings.
        /// 
        /// Don't use this exception
        /// 
        /// All values ​​must be added to the secret store
        /// 
        /// Expected configuration settings are shown below
        ///                      ↓
        /// secretKey--> Expected key for this value: "SecretKey"
        /// emailPassword--> Expected key for this value: "EmailPassword"
        /// emailAdress--> Expected key for this value: "Email"
        /// fileCryptKey--> Expected key for this value: "FileCryptKey"
        ///                      ↓
        /// The following values ​​must be added as connection strings
        ///                      ↓
        /// redisServer--> Expected key for this value: "ConnectionStrings:RedisConnection"
        /// postgres--> Expected key for this value: "ConnectionStrings:PostgresConnection"
        /// 
        /// Below are the expected value formats
        ///                ↓ 
        /// secretKey--> Expected value format: ANY
        /// emailPassword--> Expected value format: ANY
        /// emailAdress-> Expected value format: "FileCryptWeb@email.com"
        /// fileCryptKey-> Expected value format: 256-bit byte array encoded in Base64String
        /// redisServer--> Expected value format: "localhost:6379,abortConnect=false"
        /// postgres--> Expected value format: "Host=YourHost;Port=5432;Username=Username;Password=YourPassword;Database=YourDB;"
        /// </summary>

        public InvalidConfigurationException()
        : base("Invalid application configuration settings")
        {
        }
    }
}
