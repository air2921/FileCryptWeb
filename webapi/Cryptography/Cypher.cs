using System.Security.Cryptography;
using System.Text;
using webapi.Interfaces.Cryptography;

namespace webapi.Cryptography
{
    public class Cypher : ICypher
    {
        private readonly IAes _aes;
        private readonly ILogger<Cypher> _logger;

        public Cypher(IAes aes, ILogger<Cypher> logger)
        {
            _aes = aes;
            _logger = logger;
        }

        private async Task EncryptionAsync(Stream src, Stream target, byte[] key, CancellationToken cancellationToken, string? username = null, int? id = null)
        {
            try
            {
                if (username is not null && id is not null)
                {
                    string signature = $"{username}#{id}";

                    byte[] signatureBytes = Encoding.UTF8.GetBytes(signature);
                    await target.WriteAsync(signatureBytes, cancellationToken);
                }

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

        private async Task DecryptionAsync(Stream source, Stream target, byte[] key, CancellationToken cancellationToken, string? username = null, int? id = null)
        {
            try
            {
                if (username is not null && id is not null)
                {
                    string expectedSignature = $"{username}#{id}";

                    byte[] expectedSignatureBytes = Encoding.UTF8.GetBytes(expectedSignature);
                    byte[] readSignatureBytes = new byte[expectedSignatureBytes.Length];
                    await source.ReadAsync(readSignatureBytes, cancellationToken);

                    if (!readSignatureBytes.SequenceEqual(expectedSignatureBytes))
                        throw new CryptographicException("Signature verification failed.");
                }


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

        public async Task<CryptographyResult> CypherFileAsync(CryptographyData cryptoData)
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
                            await EncryptionAsync(source, target, cryptoData.Key, cryptoData.CancellationToken, cryptoData.Username, cryptoData.UserId);
                            break;
                        case "decrypt":
                            await DecryptionAsync(source, target, cryptoData.Key, cryptoData.CancellationToken, cryptoData.Username, cryptoData.UserId);
                            break;
                        default:
                            return new CryptographyResult{ Success = false };
                    }    
                }
                File.Move(tmp, cryptoData.FilePath, true);

                return new CryptographyResult { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());
                return new CryptographyResult { Success = false };
            }
        }
    }

    public class CryptographyData
    {
        public string FilePath { get; init; }
        public byte[] Key { get; init; }
        public string Operation { get; init; }
        public CancellationToken CancellationToken { get; init; }
        public string? Username { get; init; }
        public int? UserId { get; init; }
    }
}
