namespace webapi.Interfaces.Redis
{
    public interface IRedisKeys
    {
        public string PrivateKey { get; }
        public string PersonalInternalKey { get; }
        public string ReceivedInternalKey { get; }
        public string ServiceFreezeFlag { get; }
        public string MimeCollection { get; }
    }
}
