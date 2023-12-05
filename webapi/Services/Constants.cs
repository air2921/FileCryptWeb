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
}
