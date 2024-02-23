namespace webapi.Interfaces.Services
{
    public interface IImplementationFinder
    {
        T GetImplementationByKey<T>(IEnumerable<T> implementations, string key);
    }
}
