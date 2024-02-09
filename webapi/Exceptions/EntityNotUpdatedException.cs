namespace webapi.Exceptions
{
    public class EntityNotUpdatedException : Exception
    {
        public EntityNotUpdatedException(string? message = "Error when updating data") : base(message) { }
    }
}
