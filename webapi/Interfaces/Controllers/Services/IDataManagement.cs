namespace webapi.Interfaces.Controllers.Services
{
    public interface IDataManagament
    {
        public Task SetData(string key, object data);
        public Task<object> GetData(string key);
        public Task DeleteData(int id);
    }
}
