using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using webapi.Cryptography;
using webapi.Interfaces.Cryptography;

namespace tests.Cryptography_Test
{
    public class Cypher_Test
    {
        [Fact]
        public async Task CypherFile_EncryptAndDecrypt_Success()
        {
            var aesMock = new Mock<IAes>();
            aesMock.Setup(a => a.GetAesInstance()).Returns(Aes.Create());
            var logger = new Mock<ILogger<Cypher>>();
            byte[] key = new byte[32];

            var cypher = new Cypher(aesMock.Object, logger.Object);

            string originalFilePath = Path.Combine(Environment.CurrentDirectory, "CypherFile_EncryptAndDecrypt_Success.txt");

            File.WriteAllText(originalFilePath, "Test content");

            var encryptTask = await cypher.CypherFileAsync(originalFilePath, key, "encrypt", CancellationToken.None);
            Assert.True(encryptTask.Success);

            var decryptTask = await cypher.CypherFileAsync(originalFilePath, key, "decrypt", CancellationToken.None);
            Assert.True(decryptTask.Success);
            Assert.Equal("Test content", File.ReadAllText(originalFilePath));

            if (File.Exists(originalFilePath))
                File.Delete(originalFilePath);
        }

        [Theory]
        [InlineData("encrypt")]
        [InlineData("decrypt")]
        [InlineData("invalid")]
        public async Task CypherFile_EncryptionError(string operation)
        {
            var aesMock = new Mock<IAes>();
            aesMock.Setup(a => a.GetAesInstance()).Returns(Aes.Create());
            var logger = new FakeLogger<Cypher>();
            byte[] key = null;

            var cypher = new Cypher(aesMock.Object, logger);

            string originalFilePath = Path.Combine(Environment.CurrentDirectory, $"CypherFile_EncryptionError_{operation}.txt");
            File.WriteAllText(originalFilePath, "Test content");

            var encryptTask = await cypher.CypherFileAsync(originalFilePath, key, operation, CancellationToken.None);
            Assert.False(encryptTask.Success);

            if (operation.Equals("encrypt") || operation.Equals("decrypt"))
                Assert.Single(logger.LoggedMessages);

            if (File.Exists(originalFilePath))
                File.Delete(originalFilePath);
        }

        [Fact]
        public async Task CypherFile_InvalidFile()
        {
            var aesMock = new Mock<IAes>();
            aesMock.Setup(a => a.GetAesInstance()).Returns(Aes.Create());
            var logger = new FakeLogger<Cypher>();
            byte[] key = new byte[32];

            var cypher = new Cypher(aesMock.Object, logger);

            string originalFilePath = Path.Combine(Environment.CurrentDirectory, "CypherFile_InvalidFile.txt");

            var encryptTask = await cypher.CypherFileAsync(originalFilePath, key, "encrypt", CancellationToken.None);

            Assert.False(encryptTask.Success);
            Assert.Single(logger.LoggedMessages);
        }

        [Fact]
        public async Task CypherFile_DecryptError()
        {
            var aesMock = new Mock<IAes>();
            aesMock.Setup(a => a.GetAesInstance()).Returns(Aes.Create());
            var logger = new FakeLogger<Cypher>();
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

            var cypher = new Cypher(aesMock.Object, logger);

            string originalFilePath = Path.Combine(Environment.CurrentDirectory, "CypherFile_DecryptError.txt");
            File.WriteAllText(originalFilePath, "Test content");

            var encryptTask = await cypher.CypherFileAsync(originalFilePath, validKey, "encrypt", CancellationToken.None);
            Assert.True(encryptTask.Success);

            var decryptTask = await cypher.CypherFileAsync(originalFilePath, invalidKey, "decrypt", CancellationToken.None);
            Assert.False(decryptTask.Success);
            Assert.Single(logger.LoggedMessages);

            if (File.Exists(originalFilePath))
                File.Delete(originalFilePath);
        }
    }
}
