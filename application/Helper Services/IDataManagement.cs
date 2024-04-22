namespace application.Helper_Services
{
    public interface IDataManagement
    {
        public Task SetData(string key, object data);
        public Task<object> GetData(string key);
        public Task DeleteData(int id, object? parameter = null);
    }
}
