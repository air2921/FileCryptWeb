namespace webapi.Exceptions
{
    public class EntityNotCreatedException : Exception
    {
        public EntityNotCreatedException(string? message = "Error when creating entity") : base(message) { }
    }
}
