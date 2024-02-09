namespace webapi.Exceptions
{
    public class SmtpClientException : Exception
    {
        public SmtpClientException(string message = "Error sending message") : base(message) { }
    }
}
