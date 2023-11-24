namespace webapi.Localization.English
{
    public static class ExceptionFileMessages
    {
        public const string FileNotFound = "The file was not found";
        public const string NoOneFileNotFound = "No one file was found";
        public const string NoFileForUpdate = "No found file for updating";
    }

    public static class ExceptionUserMessages
    {
        public const string UserNotFound = "User was not found";
    }

    public static class ExceptionNotificationMessages
    {
        public const string NotificationNotFound = "Notification was not found";
        public const string NoOneNotificationNotFound = "No one notification was not found";
    }

    public static class ExceptionKeyMessages
    {
        public const string RecordKeysNotFound = "Keys not found";
        public const string InternalKeyNotFound = "Internal key was not found";
        public const string ReceivedKeyNotFound = "Internal key was not found";
        public const string PrivateKeyNotFound = "Private key was not found";
    }

    public static class ExceptionOfferMessages
    {
        public const string OfferNotFound = "Offer was not found";
        public const string OfferIsAccepted = "The offer was previously accepted";
    }

    public static class ExceptionMimeMessages
    {
        public const string MimeNotFound = "MIME type was not found";
    }

    public static class ExceptionPathMessages
    {
        public const string DirectoryNotFound = "The directory at the specified path does not exist";
        public const string PathNotFound = "Path to directory not found";
        public const string FolderNotEmpty = "The directory must not contain any files";
    }

    public static class ExceptionApiMessages
    {
        public const string UserApiNotFound = "This user does not have an API key";
    }

    public static class ExceptionTokenMessages
    {
        public const string NoOneSuspectRefresh = "No one suspect refresh tokens was not found";
    }
}
