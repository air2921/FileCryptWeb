namespace domain.Localization
{
    public static class NotificationMessage
    {
        private const string NEW_LINE = "|new_line|";
        private const string END_MESSAGE = "Best regards, FileCrypt Team";

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
            $"{NEW_LINE}" +
            $"{END_MESSAGE}";

        public const string AUTH_NEW_LOGIN_HEADER = "Someone has access your account !";
        public const string AUTH_NEW_LOGIN_BODY = $"Hi, Hope we didn't interfere" +
            $"{NEW_LINE}" +
            $"{NEW_LINE}" +
            $"We wanted to inform you that there has been recent activity on your account." +
            $"{NEW_LINE}" +
            $"Someone has successfully logged into your account." +
            $"{NEW_LINE}" +
            $"If this was you, no further action is required." +
            $"{NEW_LINE}" +
            $"However, if you did not authorize this login, we highly recommend securing your account immediately." +
            $"{NEW_LINE}" +
            $"Please review your account settings and consider changing your password for added security." +
            $"{NEW_LINE}" +
            $"{NEW_LINE}" +
            $"{END_MESSAGE}";

        public const string AUTH_2FA_HEADER = "Two-factor authentication status has been changed !";
        public const string AUTH_2FA_ENABLE_BODY = $"Hi, Hope we didn't interfere" +
            $"{NEW_LINE}" +
            $"{NEW_LINE}" +
            $"We wanted to inform you that two-factor authentication has been successfully enabled on your account." +
            $"{NEW_LINE}" +
            $"This is an important addition to your account security, as it requires not only your password," +
            $"but also additional verification of your identity, usually through email" +
            $"{NEW_LINE}" +
            $"We appreciate your commitment to securing your account and thank you for activating two-factor authentication." +
            $"{NEW_LINE}" +
            $"{NEW_LINE}" +
            $"If you have any questions or concerns, please feel free to reach out to our support team." +
            $"{NEW_LINE}" +
            $"Thank you for your attention to the security of your account." +
            $"{NEW_LINE}" +
            $"{NEW_LINE}" +
            $"{END_MESSAGE}";
        public const string AUTH_2FA_DISABLE_BODY = $"Hi, Hope we didn't interfere" +
            $"{NEW_LINE}" +
            $"{NEW_LINE}" +
            $"We would like to inform you that two-factor authentication has been successfully disabled on your account." +
            $"{NEW_LINE}" +
            $"We want to bring to your attention that this may increase the vulnerability of your account to unauthorized access." +
            $"{NEW_LINE}" +
            $"We strongly recommend re-enabling this feature to provide an additional layer of security for your account." +
            $"{NEW_LINE}" +
            $"{NEW_LINE}" +
            $"If you have any questions or concerns, please don't hesitate to contact our support team for further assistance." +
            $"{NEW_LINE}" +
            $"Thank you for your attention to the security of your account." +
            $"{NEW_LINE}" +
            $"{NEW_LINE}" +
            $"{END_MESSAGE}";

        public const string AUTH_EMAIL_CHANGED_HEADER = "Someone changed their email address !";
        public const string AUTH_EMAIL_CHANGED_BODY = $"Hi, Hope we didn't interfere" +
            $"{NEW_LINE}" +
            $"{NEW_LINE}" +
            $"We wanted to inform you that the email address associated with your account has been changed" +
            $"{NEW_LINE}" +
            $"If this change was made by you, no further action is required." +
            $"{NEW_LINE}" +
            $"However, if you did not make this change, we recommend taking steps to secure your account" +
            $"{NEW_LINE}" +
            $"{NEW_LINE}" +
            $"Please feel free to contact us if you have any questions or concerns." +
            $"{NEW_LINE}" +
            $"{NEW_LINE}" +
            $"{END_MESSAGE}";

        public const string AUTH_PASSWORD_CHANGED_HEADER = "Someone changed account password !";
        public const string AUTH_PASSWORD_CHANGED_BODY = $"Hi, Hope we didn't interfere" +
            $"{NEW_LINE}" +
            $"{NEW_LINE}" +
            $"We wanted to inform you that the password for your account has been successfully changed." +
            $"{NEW_LINE}" +
            $"This is an important step in ensuring the security of your account." +
            $"{NEW_LINE}" +
            $"If you made this change yourself, no further action is required." +
            $"{NEW_LINE}" +
            $"However, if you did not initiate this action, we recommend contacting support immediately to verify the security of your account." +
            $"{NEW_LINE}" +
            $"{NEW_LINE}" +
            $"Thank you for your attention to the security of your account." +
            $"{NEW_LINE}" +
            $"{NEW_LINE}" +
            $"{END_MESSAGE}";
    }
}
