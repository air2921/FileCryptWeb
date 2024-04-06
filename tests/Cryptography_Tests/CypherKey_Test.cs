using System.Security.Cryptography;
using webapi.Cryptography;
using webapi.Cryptography.Abstractions;

namespace tests.Cryptography_Tests
{
    public class CypherKey_Test
    {
        [Fact]
        public async Task EncryptAndDecryptKey_CypherKeyAsync_Success()
        {
            var aesMock = new Mock<IAes>();
            aesMock.Setup(a => a.GetAesInstance()).Returns(Aes.Create());
            string plainText = "8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8=";
            byte[] key = new byte[32];

            var encryptKey = new EncryptKey(aesMock.Object);
            var decryptKey = new DecryptKey(aesMock.Object);

            var encryptedText = await encryptKey.CypherKeyAsync(plainText, key);
            Assert.NotEqual(plainText, encryptedText);

            var decryptedText = await decryptKey.CypherKeyAsync(encryptedText, key);
            Assert.Equal(plainText, decryptedText);
        }

        [Fact]
        public async Task EncryptAndDecryptKey_CypherKeyAsync_InvalidKey()
        {
            var aesMock = new Mock<IAes>();
            aesMock.Setup(a => a.GetAesInstance()).Returns(Aes.Create());
            string plainText = "8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8=";

            byte[] validKey = new byte[]
            {
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
                0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
                0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f
            };
            byte[] invalidKey = new byte[]
            {
                0x1f, 0x1e, 0x1d, 0x1c, 0x1b, 0x1a, 0x19, 0x18,
                0x17, 0x16, 0x15, 0x14, 0x13, 0x12, 0x11, 0x10,
                0x0f, 0x0e, 0x0d, 0x0c, 0x0b, 0x0a, 0x09, 0x08,
                0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x00
            };

            var encryptKey = new EncryptKey(aesMock.Object);
            var decryptKey = new DecryptKey(aesMock.Object);

            var encryptedText = await encryptKey.CypherKeyAsync(plainText, validKey);
            Assert.NotEqual(plainText, encryptedText);

            await Assert.ThrowsAsync<CryptographicException>(async () =>
            {
                await decryptKey.CypherKeyAsync(encryptedText, invalidKey);
            });
        }

        [Fact]
        public async Task EncryptKey_CypherKeyAsync_Success()
        {
            var aesMock = new Mock<IAes>();
            aesMock.Setup(a => a.GetAesInstance()).Returns(Aes.Create());
            byte[] key = new byte[32];
            string plainText = "8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8=";

            var encryptKey = new EncryptKey(aesMock.Object);

            var encryptedText = await encryptKey.CypherKeyAsync(plainText, key);

            Assert.NotEqual(plainText, encryptedText);
        }

        [Fact]
        public async Task EncryptKey_CypherKeyAsync_InvalidText()
        {
            var aesMock = new Mock<IAes>();
            aesMock.Setup(a => a.GetAesInstance()).Returns(Aes.Create());
            byte[] key = new byte[32];
            string plainText = "dfghghdjkghjkdf";

            var encryptKey = new EncryptKey(aesMock.Object);

            await Assert.ThrowsAsync<FormatException>(async () =>
            {
                await encryptKey.CypherKeyAsync(plainText, key);
            });
        }

        [Fact]
        public async Task EncryptKey_CypherKeyAsync_InvalidKey()
        {
            var aesMock = new Mock<IAes>();
            aesMock.Setup(a => a.GetAesInstance()).Returns(Aes.Create());
            byte[] key = null;
            string plainText = "8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8=";

            var encryptKey = new EncryptKey(aesMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await encryptKey.CypherKeyAsync(plainText, key);
            });
        }

        [Fact]
        public async Task DecryptKey_CypherKeyAsync_Success()
        {
            var aesMock = new Mock<IAes>();
            aesMock.Setup(a => a.GetAesInstance()).Returns(Aes.Create());
            byte[] key = new byte[32];
            string plainText = "8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8=";

            var encryptKey = new EncryptKey(aesMock.Object);
            var decryptKey = new DecryptKey(aesMock.Object);

            var encryptedText = await encryptKey.CypherKeyAsync(plainText, key);

            var decryptedText = await decryptKey.CypherKeyAsync(encryptedText, key);

            Assert.Equal(plainText, decryptedText);
        }

        [Fact]
        public async Task DecryptKey_CypherKeyAsync_InvalidText()
        {
            byte[] key = new byte[32];
            string encryptedText = "dfghghdjkghjkdf";
            var aesMock = new Mock<IAes>();
            aesMock.Setup(a => a.GetAesInstance()).Returns(Aes.Create());

            var decryptKey = new DecryptKey(aesMock.Object);

            await Assert.ThrowsAsync<FormatException>(async () =>
            {
                await decryptKey.CypherKeyAsync(encryptedText, key);
            });
        }

        [Fact]
        public async Task DecryptKey_CypherKeyAsync_InvalidKey()
        {
            var aesMock = new Mock<IAes>();
            aesMock.Setup(a => a.GetAesInstance()).Returns(Aes.Create());
            byte[] key = null;
            string plainText = "8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8=";

            var encryptKey = new DecryptKey(aesMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await encryptKey.CypherKeyAsync(plainText, key);
            });
        }
    }
}
