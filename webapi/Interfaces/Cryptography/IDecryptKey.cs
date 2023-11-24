namespace webapi.Interfaces.Cryptography
{
    public interface IDecryptKey
    {
        Task<string> DecryptionKeyAsync(string text, byte[] key);
    }
}
