namespace webapi.Interfaces.SQL
{
    public interface IUpdate<TModel> where TModel : class
    {
        Task Update(TModel model, bool? byForeign);
    }
}
