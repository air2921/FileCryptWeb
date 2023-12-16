using System.Security.Cryptography;
using webapi.Interfaces.Cryptography;

namespace webapi.Services.Cryptography
{
    public class DecryptAsync : IDecrypt
    {
        private readonly IAes _aes;
        private readonly ILogger<DecryptAsync> _logger;

        public DecryptAsync(IAes aes, ILogger<DecryptAsync> logger)
        {
            _aes = aes;
            _logger = logger;
        }

        private async Task DecryptionAsync(Stream source, Stream target, byte[] key, CancellationToken cancellationToken)
        {
            try
            {
                using var aes = _aes.GetAesInstance();

                byte[] iv = new byte[aes.BlockSize / 8];
                await source.ReadAsync(iv, cancellationToken);
                aes.IV = iv;
                using (Rfc2898DeriveBytes rfc2898 = new(key, iv, 1000, HashAlgorithmName.SHA256))
                {
                    aes.Key = rfc2898.GetBytes(aes.KeySize / 8);
                }

                using CryptoStream cryptoStream = new(source, aes.CreateDecryptor(), CryptoStreamMode.Read);
                await cryptoStream.CopyToAsync(target, cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<CryptographyResult> DecryptFileAsync(string filePath, byte[] key, CancellationToken cancellationToken)
        {
            try
            {
                string tmp = $"{filePath}.tmp";
                using (var source = File.OpenRead(filePath))
                using (var target = File.Create(tmp))
                {
                    await DecryptionAsync(source, target, key, cancellationToken);
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
