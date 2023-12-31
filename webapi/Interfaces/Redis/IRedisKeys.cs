namespace webapi.Interfaces.Redis
{
    public interface IRedisKeys
    {
        public string PrivateKey { get; }
        public string InternalKey { get; }
        public string ReceivedKey { get; }
    }
}
