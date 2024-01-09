namespace webapi.Services
{
    public static class Constants
    {
        public static readonly TimeSpan JwtExpiry = TimeSpan.FromMinutes(20);
        public static readonly TimeSpan RefreshExpiry = TimeSpan.FromDays(90);

        #region

        /// <summary>
        /// This cookie name does not reflect its direct purpose. Such a non-obvious name may complicate possible attacks to steal authentication data
        /// </summary>
        public const string JWT_COOKIE_KEY = "session_preference";
        public const string REFRESH_COOKIE_KEY = "long_time_preference";

        #endregion

        public const string XSRF_COOKIE_KEY = ".AspNetCore.Xsrf";

        public const string SERVICE_FREEZE_FLAG = "ServiceFreezed";
        public const string MIME_COLLECTION = "MIME_Collection";

        public const string XSRF_HEADER_NAME = "X-XSRF-TOKEN";
        public const string API_HEADER_NAME = "X-API-KEY";
        public const string ENCRYPTION_KEY_HEADER_NAME = "X-ENCRYPTION-KEY";
    }

    public static class App
    {
        public const string MainDb = "PostgresConnection";
        public const string RedisDb = "RedisConnection";
        public const string appKey = "FileCryptKey";
        public const string appEmail = "Email";
        public const string appEmailPassword = "EmailPassword";
        public const string appSecretKey = "SecretKey";
    }
}
