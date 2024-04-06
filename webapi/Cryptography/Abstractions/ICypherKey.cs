namespace webapi.Cryptography.Abstractions
{
    public interface ICypherKey
    {
        Task<string> CypherKeyAsync(string text, byte[] key);
    }
}
