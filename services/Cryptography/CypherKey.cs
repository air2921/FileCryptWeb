using services.Cryptography.Abstractions;
using System.Security.Cryptography;

namespace services.Cryptography
{
    public class EncryptKey(IAes aes) : ICypherKey
    {
        private readonly IAes _aes = aes;

        public async Task<string> CypherKeyAsync(string text, byte[] key)
        {
            try
            {
                using var aes = _aes.GetAesInstance();
                byte[] iv = aes.IV;

                using var encryptor = aes.CreateEncryptor(key, iv);

                byte[] textByte = Convert.FromBase64String(text);
                var msLenght = iv.Length + textByte.Length;

                using var memoryStream = new MemoryStream(msLenght);
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using var writer = new StreamWriter(cryptoStream);
                    await writer.WriteAsync(text);
                }

                var encryptedBytes = memoryStream.ToArray();
                var result = new byte[iv.Length + encryptedBytes.Length];
                Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                Buffer.BlockCopy(encryptedBytes, 0, result, iv.Length, encryptedBytes.Length);

                return Convert.ToBase64String(result);
            }
            catch (CryptographicException)
            {
                throw;
            }
        }
    }

    public class DecryptKey : ICypherKey
    {
        private readonly IAes _aes;

        public DecryptKey(IAes aes)
        {
            _aes = aes;
        }

        public async Task<string> CypherKeyAsync(string text, byte[] key)
        {
            try
            {
                using var aes = _aes.GetAesInstance();

                byte[] cipherBytes = Convert.FromBase64String(text);
                byte[] iv = new byte[aes.IV.Length];
                byte[] encryptedData = new byte[cipherBytes.Length - iv.Length];

                Buffer.BlockCopy(cipherBytes, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(cipherBytes, iv.Length, encryptedData, 0, encryptedData.Length);

                using var decryptor = aes.CreateDecryptor(key, iv);

                using var memoryStream = new MemoryStream(encryptedData);
                using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                using var reader = new StreamReader(cryptoStream);

                return await reader.ReadToEndAsync();
            }
            catch (CryptographicException)
            {
                throw;
            }
        }
    }
}
