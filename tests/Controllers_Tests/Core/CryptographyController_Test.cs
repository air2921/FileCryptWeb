using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using webapi.Controllers.Core;
using webapi.DB.Abstractions;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Services.Abstractions;
using webapi.Services.Core;

namespace tests.Controllers_Tests.Core
{
    public class CryptographyController_Test
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task EncryptFile_Success(bool sign)
        {
            var id = 1;
            var username = sign ? "username" : null;
            var type = "private";
            var operation = "encrypt";
            var key = "key";

            var fileMock = new Mock<IFormFile>();
            var userInfoMock = new Mock<IUserInfo>();
            var redisCacheMock = new Mock<IRedisCache>();
            var providerMock = new Mock<ICryptographyProvider>();

            userInfoMock.Setup(x => x.UserId).Returns(id);
            if (sign) userInfoMock.Setup(x => x.Username).Returns(username);
            providerMock.Setup(x => x.GetCryptographyParams(type, operation))
                .ReturnsAsync(key);
            using var fileStream = new MemoryStream(Encoding.UTF8.GetBytes("test content"));
            providerMock.Setup(x => x.EncryptFile(It.Is<CryptographyOperationOptions>(x => x.File == fileMock.Object &&
            x.Type == type && x.Key == key && x.Operation == operation && x.UserID == id && x.Username == username)))
                .ReturnsAsync(new FileStreamResult(fileStream, "application/octet-stream"));

            var cryptographyController = new CryptographyController(userInfoMock.Object, redisCacheMock.Object, providerMock.Object);
            var result = await cryptographyController.EncryptFile(type, operation, sign, fileMock.Object);

            Assert.IsAssignableFrom<FileResult>(result);

            redisCacheMock.Verify(x => x.DeteteCacheByKeyPattern($"{ImmutableData.FILES_PREFIX}{id}"), Times.Once);
        }
    }
}
