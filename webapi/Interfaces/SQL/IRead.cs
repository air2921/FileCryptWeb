namespace webapi.Interfaces.SQL
{
    public interface IRead<TModel>
    {
        Task<TModel> ReadById(int id, bool? byForeign);
        Task<IEnumerable<TModel>> ReadAll();
    }
}
