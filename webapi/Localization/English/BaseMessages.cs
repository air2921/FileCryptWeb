namespace webapi.Localization.English
{
    public static class SuccessMessage
    {
        public const string SuccessDeletedAllFiles = "All files was successfully deleted";
        public const string SuccessDeletedFile = "File was successfully deleted";
        public const string SuccessUploadedFile = "File was successfully uploaded";
        public const string SuccessEncryptedFile = "Files ready to download";
        public const string ReceivedKeyRevoked = "Received key was revoked";
        public const string InternalKeyRevoked = "Internal key was revoked";
        public const string SuccessDeletedNotification = "Notification was deleted";
        public const string SuccessUpdatedFolderLenght = "Max folder lenght was updated";
        public const string SuccessDeletedUser = "User was deleted";
        public const string SuccessRoleUpdated = "Role was updated";
        public const string SuccessRefreshRevoked = "The refresh token has been revoked";
        public const string SuccessApiRevoked = "API Key has been revoked";
        public const string SuccessEmailSendedAndCreatedNotification = "Your email has been successfully sent and a notification has been created for the recipient";
        public const string SuccessCreatedNewMime = "Added new allowed MIME type";
        public const string SuccessDeletedMime = "MIME type was successfully deleted";
        public const string SuccessCreatedNewPath = "Added path to folder";
        public const string SuccessDefaultFolderUpdated = "The path to default folder was updated";
        public const string SuccessDirectoryTypeUpdated = "Directory type name was renamed";
        public const string SuccessSubdirectoryUpdated = "Subdirectories was renamed";
        public const string SuccessPathDeleted = "The path to directory was deleted";
        public const string SuccessMimeCollectionCreate = "Basic collection of MIME types added to the database";
        public const string SuccessCreatedFoldderStruct = "Basic folder structure created";
        public const string SuccessOfferDeleted = "The offer was deleted";
        public const string SuccessApiUpdated = "New API settings have been applied";
    }

    public static class ErrorMessage
    {
        public const string DeleteNoFiles = "No files available for delete";
        public const string FolderNotFound = "User folder not found";
        public const string FileNotFound = "File was not found";
        public const string DownloadNoFiles = "No files available for download";
        public const string ExceedMaxSize = "File exceeds maximum size";
        public const string EncryptNoFiles = "No files was found in directory";
        public const string InvalidKey = "Encryption key has invalid format";
        public const string NotAllFiles = "Not all files in the directory were successfully processed";
        public const string InfectedOrInvalid = "File infected, or has unsupported MIME type";
        public const string FileNoContent = "File has no content";
        public const string BadCryptographyData = "Incorrect data provided for the operation";
        public const string TaskTimedOut = "Task timed out";
        public const string NoFilesWasEncrypted = "No one file was successfuly processed";
        public const string BadIdFormat = "id should only store numbers";
        public const string HighestRoleError = "You do not have enough rights to change/delete users with the highest role in the system";
        public const string AdminCannotDelete = "You cannot remove administrators from the system";
        public const string ServiceUnavailable = "This service is currently unavailable";
    }

    public static class FileType
    {
        public const string PrivateType = "private";
        public const string InternalType = "internal";
        public const string ReceivedType = "received";
    }
}
