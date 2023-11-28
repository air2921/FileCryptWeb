namespace webapi.Interfaces.SQL
{
    public interface IUpdate<TModel>
    {
        Task Update(TModel model, bool? byForeign);
    }
}
