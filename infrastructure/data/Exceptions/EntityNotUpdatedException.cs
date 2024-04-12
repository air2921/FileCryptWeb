namespace data.Exceptions
{
    public class EntityNotUpdatedException : Exception
    {
        public EntityNotUpdatedException() { }
        public EntityNotUpdatedException(string? message = "Error when updating data") : base(message) { }
    }
}
