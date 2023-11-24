namespace webapi.Exceptions
{
    public class InvalidRouteException : Exception
    {
        public InvalidRouteException() : base("Invalid route request")
        {
        }
    }
}
