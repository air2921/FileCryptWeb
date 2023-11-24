namespace webapi.Interfaces.SQL
{
    public interface ICreate<TModel>
    {
        Task Create(TModel model);
    }
}
