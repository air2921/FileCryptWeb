namespace webapi.Interfaces.SQL
{
    public interface IRead<TModel> where TModel : class
    {
        Task<TModel> ReadById(int id, bool? byForeign);
        Task<IEnumerable<TModel>> ReadAll(int? user_id, int skip, int count);
    }
}
