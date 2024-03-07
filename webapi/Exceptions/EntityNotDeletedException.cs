namespace webapi.Exceptions
{
    public class EntityNotDeletedException : Exception
    {
        public EntityNotDeletedException() { }
        public EntityNotDeletedException(string? message = "Error when deleting data") : base(message) { }
    }
}
