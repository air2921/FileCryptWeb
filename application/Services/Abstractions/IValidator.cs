namespace application.Services.Abstractions
{
    public interface IValidator
    {
        public bool IsValid(object data, object? parameter = null);
    }
}
