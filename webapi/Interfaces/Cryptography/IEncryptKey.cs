namespace webapi.Interfaces.Cryptography
{
    public interface IEncryptKey
    {
        Task<string> EncryptionKeyAsync(string text, byte[] key);
    }
}
