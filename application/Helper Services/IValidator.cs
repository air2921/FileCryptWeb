namespace application.Helper_Services
{
    public interface IValidator
    {
        public bool IsValid(object data, object? parameter = null);
    }
}
