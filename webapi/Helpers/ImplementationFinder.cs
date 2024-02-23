using System.Reflection;
using webapi.Attributes;
using webapi.Interfaces.Services;

namespace webapi.Helpers
{
    public class ImplementationFinder : IImplementationFinder
    {
        public T GetImplementationByKey<T>(IEnumerable<T> implementations, string key)
        {
            foreach (var implementation in implementations)
            {
                var attribute = implementation.GetType().GetCustomAttribute<ImplementationKeyAttribute>();
                if (attribute is not null && attribute.Key.Equals(key))
                {
                    return implementation;
                }
            }
            throw new NotImplementedException();
        }
    }
}
