namespace webapi.Interfaces.SQL
{
    public interface IInsertBase<TModel>
    {
        Task DBInsertBase(TModel? model, bool? secure);
    }
}
