namespace webapi.Helpers
{
    public static class ImmutableData
    {
        #region Tokens Expire

        public static readonly TimeSpan JwtExpiry = TimeSpan.FromMinutes(20);
        public static readonly TimeSpan RefreshExpiry = TimeSpan.FromDays(90);

        #endregion

        #region Tokens keys in cookies

        /// <summary>
        /// This cookie name does not reflect its direct purpose. Such a non-obvious name may complicate possible attacks to steal authentication data
        /// </summary>

        public const string JWT_COOKIE_KEY = "session_preference";
        public const string REFRESH_COOKIE_KEY = "long_time_preference";
        public const string XSRF_COOKIE_KEY = ".AspNetCore.Xsrf";

        #endregion

        #region AuthCookie

        public const string USERNAME_COOKIE_KEY = "auth_username";
        public const string USER_ID_COOKIE_KEY = "auth_user_id";
        public const string ROLE_COOKIE_KEY = "auth_role";
        public const string IS_AUTHORIZED = "auth_success";

        #endregion

        #region Redis Keys

        public const string SERVICE_FREEZE_FLAG = "ServiceFreezed";
        public const string MIME_COLLECTION = "MIME_Collection";

        #endregion

        #region Request Headers

        public const string XSRF_HEADER_NAME = "X-XSRF-TOKEN";
        public const string API_HEADER_NAME = "X-API-KEY";
        public const string ENCRYPTION_KEY_HEADER_NAME = "X-ENCRYPTION-KEY";
        public const string REFRESH_TOKEN_HEADER_NAME = "X-LONG_TIME_PREFERENCE";
        public const string JWT_TOKEN_HEADER_NAME = "X-SESSION_PREFERENCE";

        #endregion

        #region Redis Keys Prefix

        public const string API_PREFIX = "API_Keys_";
        public const string FILES_PREFIX = "Files_";
        public const string KEYS_PREFIX = "Keys_";
        public const string NOTIFICATIONS_PREFIX = "Notifications_";
        public const string OFFERS_PREFIX = "Offers_";
        public const string USER_DATA_PREFIX = "User_Data_";
        public const string STORAGES_PREFIX = "Storages_";

        #endregion
    }

    public static class ImplementationKey
    {
        public const string ENCRYPT_KEY = "EncryptKey";
        public const string DECRYPT_KEY = "DecryptKey";
    }

    public static class App
    {
        public const string MAIN_DB = "PostgresConnection";
        public const string REDIS_DB = "RedisConnection";
        public const string ENCRYPTION_KEY = "FileCryptKey";
        public const string EMAIL = "Email";
        public const string EMAIL_PASSWORD = "EmailPassword";
        public const string SECRET_KEY = "SecretKey";
        public const string REACT_LAUNCH_JSON_PATH = "C:\\Users\\Stewi\\source\\repos\\FileCryptWeb\\reactapp\\.vscode\\launch.json";
    }
}
