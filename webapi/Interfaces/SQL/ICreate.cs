namespace webapi.Interfaces.SQL
{
    public interface ICreate<TModel> where TModel : class
    {
        Task Create(TModel model);
    }
}
