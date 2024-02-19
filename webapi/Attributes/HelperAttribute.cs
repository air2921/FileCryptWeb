namespace webapi.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class HelperAttribute : Attribute
    {
        public HelperAttribute() { }
    }
}
