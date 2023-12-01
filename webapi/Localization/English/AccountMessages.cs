namespace webapi.Localization.English
{
    public static class EmailMessage
    {
        public const string VerifyEmailHeader = "Verify your account on FileCryptWeb";
        public const string VerifyEmailBody = "You have selected this Email for your FileCryptWeb account.\n" +
            "If it wasn't you, please ignore this message.\n\n" +
            "Your confirm code: ";

        public const string ConfirmOldEmailHeader = "Confirm your action on FileCryptWeb";
        public const string ConfirmOldEmailBody = "You specified this email as the login for your FileCryptWeb account.\n" +
        "Someone try to change your email on FileCryptWeb.\n" +
            "If it wasn't you, please change your password on FileCryptWeb\n\n" +
            "Your confirm code: ";

        public const string ConfirmNewEmailHeader = "Verify ownership of this email address";
        public const string ConfirmNewEmailBody = "You have selected this Email as new for your FileCryptWeb account.\n" +
            "If it wasn't you, please ignore this message.\n\n" +
            "Your confirm code: ";

        public const string RecoveryAccountHeader = "Your application for account recovery has been approved";
        public const string RecoveryAccountBody = "You have applied for account recovery\n" +
            "If it was not you,please change your password.\n" +
            "Your unique recovery link: ";
    }

    public static class AccountSuccessMessage
    {
        public const string EmailSended = "An email with a confirmation code has been sent to your email";
        public const string KeyUpdated = "Key was updated";
        public const string KeyRemoved = "Key was removed";
        public const string UsernameUpdated = "Username was updated";
        public const string PasswordUpdated = "Your password was updated";
        public const string OldEmailConfirmed = "Old email was successfully confirmed";
        public const string EmailSendedRecovery = "A message has been sent to your email with a unique link to restore your account";
    }

    public static class AccountErrorMessage
    {
        public const string PasswordIncorrect = "Password is incorrect";
        public const string CodeIncorrect = "Code is incorrect";
        public const string VerifyCodeNull = "Сode timed out or an error occurred while receiving the code";
        public const string InvalidFormatPassword = "Invalid password format";
        public const string NullUserData = "Error while retrieving user data";
        public const string Error = "Unexpected error";
        public const string UserNotFound = "User was not found";
        public const string UserExists = "User already exists";
        public const string Forbidden = "Action prohibited";
        public const string InvalidToken = "Invalid token";
    }
}
