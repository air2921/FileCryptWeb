namespace webapi.Interfaces.Cryptography
{
    public interface ICypherKey
    {
        Task<string> CypherKeyAsync(string text, byte[] key);
    }
}
