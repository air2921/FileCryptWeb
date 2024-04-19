namespace application.Abstractions.Services.Inner
{
    public interface ICryptographyHelper
    {
        public Task CypherFile(string filePath, string operation, byte[] key);
        public Task<byte[]?> GetKey(int userId, int keyId, int storageId, string accessCode);
    }
}
