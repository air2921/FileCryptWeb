namespace webapi.Services
{
    public static class Constants
    {
        public const int JWT_EXPIRY = 20;
        public const int REFRESH_EXPIRY = 90;

        public const string JWT_COOKIE_KEY = "auth_access";
        public const string REFRESH_COOKIE_KEY = "auth_refresh";

        public const string SERVICE_FREEZE_FLAG = "ServiceFreezed";
        public const string MIME_COLLECTION = "MIME_Collection";
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
