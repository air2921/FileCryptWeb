namespace data.Exceptions
{
    public class EntityNotCreatedException : Exception
    {
        public EntityNotCreatedException() { }
        public EntityNotCreatedException(string? message = "Error when creating entity") : base(message) { }
    }
}
