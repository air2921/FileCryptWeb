namespace application.Abstractions.Services.TP_Services
{
    public interface ICypherKey
    {
        Task<string> CypherKeyAsync(string text, byte[] key);
    }
}
