namespace application.Helpers
{
    public static class ImmutableData
    {
        #region Tokens Expire

        public static readonly TimeSpan JwtExpiry = TimeSpan.FromMinutes(90);
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
        public const string REFRESH_TOKEN_HEADER_NAME = "X-REFRESH";
        public const string NONE_BEARER = "X-BEARER";

        #endregion

        #region Redis Keys Prefix

        public const string FILES_PREFIX = "Files_";
        public const string KEYS_PREFIX = "Keys_";
        public const string NOTIFICATIONS_PREFIX = "Notifications_";
        public const string OFFERS_PREFIX = "Offers_";
        public const string USER_DATA_PREFIX = "User_Data_";
        public const string STORAGES_PREFIX = "Storages_";
        public const string STORAGE_ITEMS_PREFIX = "StorageItems_";
        public const string USER_LIST = "UserListByUsername_";

        #endregion
    }

    public static class ImplementationKey
    {
        public const string ENCRYPT_KEY = "EncryptKey";
        public const string DECRYPT_KEY = "DecryptKey";

        public const string ACCOUNT_2FA_SERVICE = "Account_2FaServiceImplementation";
        public const string ACCOUNT_EMAIL_SERVICE = "Account_EmailServiceImplementation";
        public const string ACCOUNT_PASSWORD_SERVICE = "Account_PasswordServiceImplementation";
        public const string ACCOUNT_USERNAME_SERVICE = "Account_UsernameServiceImplementation";
        public const string ACCOUNT_REGISTRATION_SERVICE = "Account_RegistrationServiceImplementation";
        public const string ACCOUNT_SESSION_SERVICE = "Account_SessionServiceImplementation";
        public const string ACCOUNT_RECOVERY_SERVICE = "Account_RecoveryServiceImplementation";

        public const string CORE_OFFER_SERVICE = "Core_OfferServiceImplementation";
        public const string CORE_KEY_STORAGE_SERVICE = "Core_KeyStorageServiceImplementation";
        public const string CORE_KEY_SERVICE = "Core_KeyServiceImplementation";

        public const string ADMIN_TOKEN_SERVICE = "Admin_TokenServiceImplementation";
        public const string ADMIN_USER_SERVICE = "Admin_UserServiceImplementation";
    }

    public static class App
    {
        public const string MAIN_DB = "Postgres";
        public const string REDIS_DB = "Redis";
        public const string ELASTIC_SEARCH = "Elasticsearch";
        public const string CLAM_SERVER = "ClamServer";
        public const string CLAM_PORT = "ClamPort";
        public const string ENCRYPTION_KEY = "AppKey";
        public const string EMAIL = "Email";
        public const string EMAIL_PASSWORD = "EmailPassword";
        public const string REACT = "";
        public const string SECRET_KEY = "JwtKey";
        public const string ISSUER = "JwtIssuer";
        public const string AUDIENCE = "JwtAudience";
    }

    public static class RegularEx
    {
        public const string EncryptionKey = "^[A-Za-z0-9+/]{43}=$";
        public const string Password = "^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{8,30}$";
        public const string Email = "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$";
        public const string Username = "^(?!.*#)[\\s\\S]{1,30}$";
    }
}
