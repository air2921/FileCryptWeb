namespace webapi.Localization
{
    public static class NotificationMessage
    {
        private const string NEW_LINE = "|new_line|";

        public const string AUTH_TOKENS_REVOKED_HEADER = "Revocation of Authentication Tokens Due to Suspicious Activity";
        public const string AUTH_TOKENS_REVOKED_BODY = $"Hi, Hope we didn't interfere !" +
            $"{NEW_LINE}" +
            $"{NEW_LINE}" +
            $"We regret to inform you that your authentication tokens have been revoked due to suspicious activity detected on your account." +
            $"{NEW_LINE}" +
            $"This measure is taken to ensure the security of your account and prevent unauthorized access." +
            $"{NEW_LINE}" +
            $"To regain access to your account, please follow the steps outlined in our security protocol:" +
            $"{NEW_LINE}" +
            $"We understand the inconvenience this may cause, but your security is our top priority." +
            $"{NEW_LINE}" +
            $"If you have any concerns or require assistance, please do not hesitate to contact our support team." +
            $"{NEW_LINE}" +
            $"{NEW_LINE}" +
            $"Thank you for your understanding and cooperation." +
            $"{NEW_LINE}" +
            $"Best regards {NEW_LINE}" +
            $"FileCrypt Team";

    }
}
