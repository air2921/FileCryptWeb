namespace webapi.Exceptions
{
    public class InvalidConfigurationException : Exception
    {
        /// <summary>
        /// An exception is only needed for forwarding in case of invalid configuration settings.
        /// </summary>

        public InvalidConfigurationException()
        : base("Invalid application configuration settings")
        {
        }
    }
}
