namespace webapi.Attributes
{
    public class ImplementationKeyAttribute : Attribute
    {
        public string Key { get; }

        public ImplementationKeyAttribute(string key)
        {
            Key = key;
        }
    }
}
