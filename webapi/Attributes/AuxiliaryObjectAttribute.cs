namespace webapi.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class AuxiliaryObjectAttribute : Attribute
    {
        public AuxiliaryObjectAttribute() { }
    }
}
