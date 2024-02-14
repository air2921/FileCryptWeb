namespace webapi.Services
{
    public static class Constants
    {
        public static readonly TimeSpan JwtExpiry = TimeSpan.FromMinutes(20);
        public static readonly TimeSpan RefreshExpiry = TimeSpan.FromDays(90);

        #region Tokens keys in cookies

        /// <summary>
        /// This cookie name does not reflect its direct purpose. Such a non-obvious name may complicate possible attacks to steal authentication data
        /// </summary>

        public const string JWT_COOKIE_KEY = "session_preference";
        public const string REFRESH_COOKIE_KEY = "long_time_preference";
        public const string XSRF_COOKIE_KEY = ".AspNetCore.Xsrf";

        #endregion

        public const string USERNAME_COOKIE_KEY = "auth_username";
        public const string USER_ID_COOKIE_KEY = "auth_user_id";
        public const string ROLE_COOKIE_KEY = "auth_role";
        public const string IS_AUTHORIZED = "auth_success";

        public const string SERVICE_FREEZE_FLAG = "ServiceFreezed";
        public const string MIME_COLLECTION = "MIME_Collection";

        public const string XSRF_HEADER_NAME = "X-XSRF-TOKEN";
        public const string API_HEADER_NAME = "X-API-KEY";
        public const string ENCRYPTION_KEY_HEADER_NAME = "X-ENCRYPTION-KEY";
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
