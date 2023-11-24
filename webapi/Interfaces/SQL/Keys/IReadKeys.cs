namespace webapi.Interfaces.SQL.Keys
{
    public interface IReadKeys
    {
        Task<string> ReadPersonalInternalKey(int userid);
        Task<string> ReadReceivedInternalKey(int userid);
        Task<string> ReadPrivateKey(int userid);
    }
}
