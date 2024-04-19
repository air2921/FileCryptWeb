using Microsoft.Extensions.Logging;
using services.Cryptography.Abstractions;
using System.Security.Cryptography;
using System.Text;
using application.DTO;
using application.Abstractions.Services.TP_Services;

namespace services.Cryptography
{
    public class Cypher(IAes aes, ILogger<Cypher> logger) : ICypher
    {
        private readonly IAes _aes = aes;

        private async Task EncryptionAsync(Stream src, Stream target, byte[] key,
            CancellationToken cancellationToken/*, string? signature*/)
        {
            try
            {
                //if (signature is not null)
                //    await target.WriteAsync(Encoding.UTF8.GetBytes(signature), cancellationToken);

                using var aes = _aes.GetAesInstance();

                byte[] iv = aes.IV;
                await target.WriteAsync(iv, cancellationToken);
                using (Rfc2898DeriveBytes rfc2898 = new(key, iv, 1000, HashAlgorithmName.SHA256))
                    aes.Key = rfc2898.GetBytes(aes.KeySize / 8);

                using CryptoStream cryptoStream = new(target, aes.CreateEncryptor(), CryptoStreamMode.Write);
                await src.CopyToAsync(cryptoStream, cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task DecryptionAsync(Stream source, Stream target, byte[] key,
            CancellationToken cancellationToken/*, string? signature*/)
        {
            try
            {
                //if (signature is not null)
                //{
                //    byte[] expectedSignatureBytes = Encoding.UTF8.GetBytes(signature);
                //    byte[] readSignatureBytes = new byte[expectedSignatureBytes.Length];
                //    await source.ReadAsync(readSignatureBytes, cancellationToken);

                //    if (!readSignatureBytes.SequenceEqual(expectedSignatureBytes))
                //        throw new CryptographicException("Signature verification failed.");
                //}

                using var aes = _aes.GetAesInstance();

                byte[] iv = new byte[aes.BlockSize / 8];
                await source.ReadAsync(iv, cancellationToken);
                aes.IV = iv;
                using (Rfc2898DeriveBytes rfc2898 = new(key, iv, 1000, HashAlgorithmName.SHA256))
                    aes.Key = rfc2898.GetBytes(aes.KeySize / 8);

                using CryptoStream cryptoStream = new(source, aes.CreateDecryptor(), CryptoStreamMode.Read);
                await cryptoStream.CopyToAsync(target, cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<CryptographyResult> CypherFileAsync(CryptographyDTO cryptoData)
        {
            try
            {
                string tmp = $"{cryptoData.FilePath}.tmp";
                using (var source = File.OpenRead(cryptoData.FilePath))
                using (var target = File.Create(tmp))
                {
                    switch (cryptoData.Operation)
                    {
                        case "encrypt":
                            await EncryptionAsync(source, target, cryptoData.Key, cryptoData.CancellationToken);
                            break;
                        case "decrypt":
                            await DecryptionAsync(source, target, cryptoData.Key, cryptoData.CancellationToken);
                            break;
                        default:
                            return new CryptographyResult { Success = false };
                    }
                }
                File.Move(tmp, cryptoData.FilePath, true);

                return new CryptographyResult { Success = true };
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex.ToString());
                return new CryptographyResult { Success = false };
            }
        }
    }
}
