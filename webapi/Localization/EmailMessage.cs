namespace webapi.Localization
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

        public const string Verify2FaHeader = "Confirm login on FileCryptWeb";
        public const string Verify2FaBody = "You provide this email address to confirm your two-factor authentication login.\n Your login code: ";

        public const string Change2FaHeader = "Confirmation Code for Two-Factor Authentication Status Change";
        public const string Change2FaBody = "We have received a request to change the status of two-factor authentication for your account.\n" +
            "To complete this action, please use the following confirmation code: ";
    }
}
