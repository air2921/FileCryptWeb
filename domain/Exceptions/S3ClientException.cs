namespace domain.Exceptions
{
    public class S3ClientException : Exception
    {
        public S3ClientException(string message) : base(message)
        {  
        }
    }
}
