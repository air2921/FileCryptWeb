
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using webapi.Controllers.Core;
using webapi.Cryptography.Abstractions;
using webapi.DB.Abstractions;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Models;
using webapi.Services.Abstractions;
using webapi.Services.Core;
using webapi.Services.Core.Data_Handlers;

namespace tests.Controllers_Tests.Core
{
    public class KeysController_Test
    {
        [Fact]
        public async Task GetAllKeys_Success()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var cacheHandlerMock = new Mock<ICacheHandler<KeyModel>>();
            var cypherKeyMock = new Mock<ICypherKey>();
            var configurationMock = new Mock<IConfiguration>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            cacheHandlerMock.Setup(x => x.CacheAndGet(It.IsAny<KeyObject>())).ReturnsAsync(new KeyModel());
            cypherKeyMock.Setup(x => x.CypherKeyAsync(It.IsAny<string>(), It.IsAny<byte[]>())).ReturnsAsync("");
            configurationMock.Setup(x => x[App.ENCRYPTION_KEY]).Returns("8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8=");

            var keysController = new KeysController(null, cacheHandlerMock.Object, configurationMock.Object,
                null, null, userInfoMock.Object, cypherKeyMock.Object, null, null, null);

            var result = await keysController.GetAllKeys();

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetAllKeys_NotFound()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var cacheHandlerMock = new Mock<ICacheHandler<KeyModel>>();
            var configurationMock = new Mock<IConfiguration>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            cacheHandlerMock.Setup(x => x.CacheAndGet(It.IsAny<KeyObject>())).ReturnsAsync((KeyModel)null);
            configurationMock.Setup(x => x[App.ENCRYPTION_KEY]).Returns("8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8=");

            var keysController = new KeysController(null, cacheHandlerMock.Object, configurationMock.Object,
                null, null, userInfoMock.Object, null, null, null, null);

            var result = await keysController.GetAllKeys();

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Theory]
        [InlineData(typeof(OperationCanceledException))]
        [InlineData(typeof(FormatException))]
        public async Task GetAllKeys_ThrowsExceptions(Type ex)
        {
            var userInfoMock = new Mock<IUserInfo>();
            var cacheHandlerMock = new Mock<ICacheHandler<KeyModel>>();
            var configurationMock = new Mock<IConfiguration>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            cacheHandlerMock.Setup(x => x.CacheAndGet(It.IsAny<KeyObject>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(ex));
            configurationMock.Setup(x => x[App.ENCRYPTION_KEY]).Returns("8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8=");

            var keysController = new KeysController(null, cacheHandlerMock.Object, configurationMock.Object,
                null, null, userInfoMock.Object, null, null, null, null);

            var result = await keysController.GetAllKeys();

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UpdatePrivateKey_Success(bool valid)
        {
            var userInfoMock = new Mock<IUserInfo>();
            var redisKeysMock = new Mock<IRedisKeys>();
            var generateMock = new Mock<IGenerate>();
            var validatorMock = new Mock<IValidator>();
            var helpersMock = new Mock<IKeyHelper>();
            var dataManagementMock = new Mock<IDataManagement>();
            var configurationMock = new Mock<IConfiguration>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(valid);
            configurationMock.Setup(x => x[App.ENCRYPTION_KEY]).Returns("8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8=");
            userInfoMock.Setup(x => x.UserId).Returns(1);
            redisKeysMock.Setup(x => x.PrivateKey).Returns("");

            var keysController = new KeysController(null, null, configurationMock.Object,
                generateMock.Object, redisKeysMock.Object, userInfoMock.Object, null,
                validatorMock.Object, dataManagementMock.Object, helpersMock.Object);

            var result = await keysController.UpdatePrivateKey("");

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);

            dataManagementMock.Verify(x => x.DeleteData(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            helpersMock.Verify(x => x.UpdateKey(It.IsAny<string>(), It.IsAny<int>(), FileType.Private), Times.Once);
        }

        [Theory]
        [InlineData(typeof(EntityNotUpdatedException))]
        [InlineData(typeof(CryptographicException))]
        public async Task UpdatePrivateKey_ThrowsExceptions(Type ex)
        {
            var userInfoMock = new Mock<IUserInfo>();
            var redisKeysMock = new Mock<IRedisKeys>();
            var generateMock = new Mock<IGenerate>();
            var validatorMock = new Mock<IValidator>();
            var helpersMock = new Mock<IKeyHelper>();
            var dataManagementMock = new Mock<IDataManagement>();
            var configurationMock = new Mock<IConfiguration>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);
            configurationMock.Setup(x => x[App.ENCRYPTION_KEY]).Returns("8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8=");
            userInfoMock.Setup(x => x.UserId).Returns(1);
            redisKeysMock.Setup(x => x.PrivateKey).Returns("");
            helpersMock.Setup(x => x.UpdateKey(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileType>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(ex));

            var keysController = new KeysController(null, null, configurationMock.Object,
                generateMock.Object, redisKeysMock.Object, userInfoMock.Object, null,
                validatorMock.Object, dataManagementMock.Object, helpersMock.Object);

            var result = await keysController.UpdatePrivateKey("");

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UpdateInternalKey_Success(bool valid)
        {
            var userInfoMock = new Mock<IUserInfo>();
            var redisKeysMock = new Mock<IRedisKeys>();
            var generateMock = new Mock<IGenerate>();
            var validatorMock = new Mock<IValidator>();
            var helpersMock = new Mock<IKeyHelper>();
            var dataManagementMock = new Mock<IDataManagement>();
            var configurationMock = new Mock<IConfiguration>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(valid);
            configurationMock.Setup(x => x[App.ENCRYPTION_KEY]).Returns("8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8=");
            userInfoMock.Setup(x => x.UserId).Returns(1);
            redisKeysMock.Setup(x => x.InternalKey).Returns("");

            var keysController = new KeysController(null, null, configurationMock.Object,
                generateMock.Object, redisKeysMock.Object, userInfoMock.Object, null,
                validatorMock.Object, dataManagementMock.Object, helpersMock.Object);

            var result = await keysController.UpdatePersonalInternalKey("");

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);

            dataManagementMock.Verify(x => x.DeleteData(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            helpersMock.Verify(x => x.UpdateKey(It.IsAny<string>(), It.IsAny<int>(), FileType.Internal), Times.Once);
        }

        [Theory]
        [InlineData(typeof(EntityNotUpdatedException))]
        [InlineData(typeof(CryptographicException))]
        public async Task UpdateInternalKey_ThrowsExceptions(Type ex)
        {
            var userInfoMock = new Mock<IUserInfo>();
            var redisKeysMock = new Mock<IRedisKeys>();
            var generateMock = new Mock<IGenerate>();
            var validatorMock = new Mock<IValidator>();
            var helpersMock = new Mock<IKeyHelper>();
            var dataManagementMock = new Mock<IDataManagement>();
            var configurationMock = new Mock<IConfiguration>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);
            configurationMock.Setup(x => x[App.ENCRYPTION_KEY]).Returns("8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8=");
            userInfoMock.Setup(x => x.UserId).Returns(1);
            redisKeysMock.Setup(x => x.InternalKey).Returns("");
            helpersMock.Setup(x => x.UpdateKey(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileType>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(ex));

            var keysController = new KeysController(null, null, configurationMock.Object,
                generateMock.Object, redisKeysMock.Object, userInfoMock.Object, null,
                validatorMock.Object, dataManagementMock.Object, helpersMock.Object);

            var result = await keysController.UpdatePersonalInternalKey("");

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task CleanReceivedInternalKey_Success()
        {
            var configurationMock = new Mock<IConfiguration>();
            var userInfoMock = new Mock<IUserInfo>();
            var redisKeysMock = new Mock<IRedisKeys>();
            var dataManagementMock = new Mock<IDataManagement>();
            var cacheHandlerMock = new Mock<ICacheHandler<KeyModel>>();
            var keyRepositoryMock = new Mock<IRepository<KeyModel>>();

            configurationMock.Setup(x => x[App.ENCRYPTION_KEY]).Returns("8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8=");
            userInfoMock.Setup(x => x.UserId).Returns(1);
            redisKeysMock.Setup(x => x.ReceivedKey).Returns("");
            cacheHandlerMock.Setup(x => x.CacheAndGet(It.IsAny<KeyObject>())).ReturnsAsync(new KeyModel { received_key = "key" });

            var keysController = new KeysController(keyRepositoryMock.Object, cacheHandlerMock.Object, configurationMock.Object,
                null, redisKeysMock.Object, userInfoMock.Object, null,
                null, dataManagementMock.Object, null);

            var result = await keysController.CleanReceivedInternalKey();

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);

            dataManagementMock.Verify(x => x.DeleteData(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            keyRepositoryMock.Verify(x => x.Update(It.IsAny<KeyModel>(), CancellationToken.None), Times.Once);
        }


        [Fact]
        public async Task CleanReceivedInternalKey_NotFound()
        {
            var configurationMock = new Mock<IConfiguration>();
            var userInfoMock = new Mock<IUserInfo>();
            var redisKeysMock = new Mock<IRedisKeys>();
            var dataManagementMock = new Mock<IDataManagement>();
            var cacheHandlerMock = new Mock<ICacheHandler<KeyModel>>();
            var keyRepositoryMock = new Mock<IRepository<KeyModel>>();

            configurationMock.Setup(x => x[App.ENCRYPTION_KEY]).Returns("8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8=");
            userInfoMock.Setup(x => x.UserId).Returns(1);
            redisKeysMock.Setup(x => x.ReceivedKey).Returns("");
            cacheHandlerMock.Setup(x => x.CacheAndGet(It.IsAny<KeyObject>())).ReturnsAsync((KeyModel)null);

            var keysController = new KeysController(keyRepositoryMock.Object, cacheHandlerMock.Object, configurationMock.Object,
                null, redisKeysMock.Object, userInfoMock.Object, null,
                null, dataManagementMock.Object, null);

            var result = await keysController.CleanReceivedInternalKey();

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task CleanReceivedInternalKey_ReceivedKeyAlreadyNull()
        {
            var configurationMock = new Mock<IConfiguration>();
            var userInfoMock = new Mock<IUserInfo>();
            var redisKeysMock = new Mock<IRedisKeys>();
            var dataManagementMock = new Mock<IDataManagement>();
            var cacheHandlerMock = new Mock<ICacheHandler<KeyModel>>();
            var keyRepositoryMock = new Mock<IRepository<KeyModel>>();

            configurationMock.Setup(x => x[App.ENCRYPTION_KEY]).Returns("8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8=");
            userInfoMock.Setup(x => x.UserId).Returns(1);
            redisKeysMock.Setup(x => x.ReceivedKey).Returns("");
            cacheHandlerMock.Setup(x => x.CacheAndGet(It.IsAny<KeyObject>())).ReturnsAsync(new KeyModel { received_key = null });

            var keysController = new KeysController(keyRepositoryMock.Object, cacheHandlerMock.Object, configurationMock.Object,
                null, redisKeysMock.Object, userInfoMock.Object, null,
                null, dataManagementMock.Object, null);

            var result = await keysController.CleanReceivedInternalKey();

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(409, objectResult.StatusCode);
        }

        [Theory]
        [InlineData(typeof(OperationCanceledException))]
        [InlineData(typeof(EntityNotUpdatedException))]
        [InlineData(typeof(FormatException))]
        public async Task CleanReceivedInternalKey_ThrowsExceptions(Type ex)
        {
            var configurationMock = new Mock<IConfiguration>();
            var userInfoMock = new Mock<IUserInfo>();
            var redisKeysMock = new Mock<IRedisKeys>();
            var dataManagementMock = new Mock<IDataManagement>();
            var cacheHandlerMock = new Mock<ICacheHandler<KeyModel>>();
            var keyRepositoryMock = new Mock<IRepository<KeyModel>>();

            configurationMock.Setup(x => x[App.ENCRYPTION_KEY]).Returns("8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8=");
            userInfoMock.Setup(x => x.UserId).Returns(1);
            redisKeysMock.Setup(x => x.ReceivedKey).Returns("");
            cacheHandlerMock.Setup(x => x.CacheAndGet(It.IsAny<KeyObject>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(ex));

            var keysController = new KeysController(keyRepositoryMock.Object, cacheHandlerMock.Object, configurationMock.Object,
                null, redisKeysMock.Object, userInfoMock.Object, null,
                null, dataManagementMock.Object, null);

            var result = await keysController.CleanReceivedInternalKey();

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
