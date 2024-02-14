using System.Security.Cryptography;
using webapi.Interfaces.Cryptography;

namespace webapi.Cryptography
{
    public class EncryptAsync : IEncrypt
    {
        private readonly IAes _aes;
        private readonly ILogger<EncryptAsync> _logger;

        public EncryptAsync(IAes aes, ILogger<EncryptAsync> logger)
        {
            _aes = aes;
            _logger = logger;
        }

        private async Task EncryptionAsync(Stream src, Stream target, byte[] key, CancellationToken cancellationToken)
        {
            try
            {
                using var aes = _aes.GetAesInstance();

                byte[] iv = aes.IV;
                await target.WriteAsync(iv, cancellationToken);
                using (Rfc2898DeriveBytes rfc2898 = new(key, iv, 1000, HashAlgorithmName.SHA256))
                {
                    aes.Key = rfc2898.GetBytes(aes.KeySize / 8);
                }

                using CryptoStream cryptoStream = new(target, aes.CreateEncryptor(), CryptoStreamMode.Write);
                await src.CopyToAsync(cryptoStream, cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<CryptographyResult> EncryptFileAsync(string filePath, byte[] key, CancellationToken cancellationToken)
        {
            string tmp = $"{filePath}.tmp";
            try
            {
                using (var source = File.OpenRead(filePath))
                using (var target = File.Create(tmp))
                {
                    await EncryptionAsync(source, target, key, cancellationToken);
                }
                File.Move(tmp, filePath, true);

                return new CryptographyResult { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());
                return new CryptographyResult { Success = false };
            }
        }
    }
}
