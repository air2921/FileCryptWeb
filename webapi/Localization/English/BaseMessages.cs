namespace webapi.Localization.English
{
    public static class SuccessMessage
    {
        public const string ReceivedKeyRevoked = "Received key was revoked";
        public const string InternalKeyRevoked = "Internal key was revoked";
        public const string SuccessDeletedNotification = "Notification was deleted";
        public const string SuccessDeletedUser = "User was deleted";
        public const string SuccessRoleUpdated = "Role was updated";
        public const string SuccessRefreshRevoked = "The refresh token has been revoked";
        public const string SuccessApiRevoked = "API Key has been revoked";
        public const string SuccessEmailSendedAndCreatedNotification = "Your email has been successfully sent and a notification has been created for the recipient";
        public const string SuccessCreatedNewMime = "Added new allowed MIME type";
        public const string SuccessDeletedMime = "MIME type was successfully deleted";
        public const string SuccessMimeCollectionCreate = "Basic collection of MIME types added to the database";
        public const string SuccessCreatedFoldderStruct = "Basic folder structure created";
        public const string SuccessOfferDeleted = "The offer was deleted";
        public const string SuccessOfferAccepted = "The offer was accepted";
        public const string SuccessApiUpdated = "New API settings have been applied";
    }

    public static class ErrorMessage
    {
        public const string ExceedMaxSize = "File exceeds maximum size";
        public const string FileNotFound = "File was not found";
        public const string InvalidKey = "Encryption key has invalid format";
        public const string InfectedOrInvalid = "File infected, or has unsupported MIME type";
        public const string FileNoContent = "File has no content";
        public const string BadCryptographyData = "Incorrect data provided for the operation";
        public const string TaskTimedOut = "Task timed out";
        public const string HighestRoleError = "You do not have enough rights to change/delete users with the highest role in the system";
        public const string AdminCannotDelete = "You cannot remove administrators from the system";
    }

    public static class FileType
    {
        public const string PrivateType = "private";
        public const string InternalType = "internal";
        public const string ReceivedType = "received";
    }
}
