namespace webapi.Localization
{
    public static class ErrorMessage
    {
        public const string ExceedMaxSize = "File exceeds maximum size";
        public const string FileNotFound = "File was not found";
        public const string InvalidKey = "Encryption key has invalid format";
        public const string InfectedOrInvalid = "File infected, or has unsupported MIME type";
        public const string FileNoContent = "File has no content";
        public const string BadCryptographyData = "Incorrect cryptodata provided for the operation";
        public const string TaskTimedOut = "Task timed out";
        public const string HighestRoleError = "You do not have enough rights to change/delete users with the highest role in the system";
        public const string AdminCannotDelete = "You cannot remove administrators from the system";
    }
}
