namespace webapi.Interfaces.Controllers.Services
{
    public interface ICryptographyHelper
    {
        public byte[] CheckAndConvertKey(string key);
        public Task EncryptFile(string filePath, string operation, byte[] key, int? id, string? username);
        public Task<string> CacheKey(string key, int userId);
    }
}
