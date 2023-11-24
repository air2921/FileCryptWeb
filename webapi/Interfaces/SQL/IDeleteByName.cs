namespace webapi.Interfaces.SQL
{
    public interface IDeleteByName<TModel>
    {
        Task DeleteByName(string name);
    }
}
