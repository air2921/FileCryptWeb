namespace domain.Abstractions.Services
{
    public interface ICypherKey
    {
        Task<string> CypherKeyAsync(string text, byte[] key);
    }
}
