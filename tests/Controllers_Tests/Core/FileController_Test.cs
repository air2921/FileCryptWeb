using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Core;
using webapi.DB.Abstractions;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Models;
using webapi.Services.Abstractions;
using webapi.Services.Core.Data_Handlers;

namespace tests.Controllers_Tests.Core
{
    public class FileController_Test
    {
        [Fact]
        public async Task DeleteFileFromHistory_CacheDeleted_Success()
        {
            var fileRepositoryMock = new Mock<IRepository<FileModel>>();
            var redisCacheMock = new Mock<IRedisCache>();
            var userInfoMock = new Mock<IUserInfo>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            fileRepositoryMock.Setup(x => x.DeleteByFilter(It.IsAny<Func<IQueryable<FileModel>, IQueryable<FileModel>>>(), CancellationToken.None))
                .ReturnsAsync(new FileModel());

            var fileController = new FileController(fileRepositoryMock.Object, redisCacheMock.Object, userInfoMock.Object,
                null);

            var result = await fileController.DeleteFileFromHistory(1);

            fileRepositoryMock.Verify(repo => repo
                .DeleteByFilter(It.IsAny<Func<IQueryable<FileModel>, IQueryable<FileModel>>>(), CancellationToken.None), Times.Once);
            redisCacheMock.Verify(cache => cache.DeteteCacheByKeyPattern($"{ImmutableData.FILES_PREFIX}{1}"), Times.Once);
            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task DeleteFileFromHistory_CacheNotDeleted_Success()
        {
            var fileRepositoryMock = new Mock<IRepository<FileModel>>();
            var redisCacheMock = new Mock<IRedisCache>();
            var userInfoMock = new Mock<IUserInfo>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            fileRepositoryMock.Setup(x => x.DeleteByFilter(It.IsAny<Func<IQueryable<FileModel>, IQueryable<FileModel>>>(), CancellationToken.None))
                .ReturnsAsync((FileModel)null);

            var fileController = new FileController(fileRepositoryMock.Object, redisCacheMock.Object, userInfoMock.Object,
                null);

            var result = await fileController.DeleteFileFromHistory(1);

            fileRepositoryMock.Verify(repo => repo
                .DeleteByFilter(It.IsAny<Func<IQueryable<FileModel>, IQueryable<FileModel>>>(), CancellationToken.None), Times.Once);
            redisCacheMock.Verify(cache => cache.DeteteCacheByKeyPattern(It.IsAny<string>()), Times.Never);
            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task DeleteFileFromHistory_NotDeleted()
        {
            var fileRepositoryMock = new Mock<IRepository<FileModel>>();
            var redisCacheMock = new Mock<IRedisCache>();
            var userInfoMock = new Mock<IUserInfo>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            fileRepositoryMock.Setup(x => x.DeleteByFilter(It.IsAny<Func<IQueryable<FileModel>, IQueryable<FileModel>>>(), CancellationToken.None))
                .ThrowsAsync(new EntityNotDeletedException());

            var fileController = new FileController(fileRepositoryMock.Object, redisCacheMock.Object, userInfoMock.Object,
                null);

            var result = await fileController.DeleteFileFromHistory(1);

            redisCacheMock.Verify(cache => cache.DeteteCacheByKeyPattern(It.IsAny<string>()), Times.Never);
            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetOneFile_Success()
        {
            var cacheHandlerMock = new Mock<ICacheHandler<FileModel>>();
            var userInfoMock = new Mock<IUserInfo>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            cacheHandlerMock.Setup(x => x.CacheAndGet(It.IsAny<FileObject>())).ReturnsAsync(new FileModel());

            var fileController = new FileController(null, null, userInfoMock.Object, cacheHandlerMock.Object);
            var result = await fileController.GetOneFile(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetOneFile_NotFound()
        {
            var cacheHandlerMock = new Mock<ICacheHandler<FileModel>>();
            var userInfoMock = new Mock<IUserInfo>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            cacheHandlerMock.Setup(x => x.CacheAndGet(It.IsAny<FileObject>())).ReturnsAsync((FileModel)null);

            var fileController = new FileController(null, null, userInfoMock.Object, cacheHandlerMock.Object);
            var result = await fileController.GetOneFile(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Theory]
        [InlineData(typeof(OperationCanceledException))]
        [InlineData(typeof(FormatException))]
        public async Task GetOneFile_ThrowsExceptions(Type ex)
        {
            var cacheHandlerMock = new Mock<ICacheHandler<FileModel>>();
            var userInfoMock = new Mock<IUserInfo>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            cacheHandlerMock.Setup(x => x.CacheAndGet(It.IsAny<FileObject>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(ex));

            var fileController = new FileController(null, null, userInfoMock.Object, cacheHandlerMock.Object);
            var result = await fileController.GetOneFile(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetAllFiles_Success()
        {
            var cacheHandlerMock = new Mock<ICacheHandler<FileModel>>();
            var userInfoMock = new Mock<IUserInfo>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            cacheHandlerMock.Setup(x => x.CacheAndGetRange(It.IsAny<FileRangeObject>())).ReturnsAsync(new List<FileModel>());

            var fileController = new FileController(null, null, userInfoMock.Object, cacheHandlerMock.Object);
            var result = await fileController.GetAllFiles(0, 5, true, string.Empty, string.Empty, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Theory]
        [InlineData(typeof(OperationCanceledException))]
        [InlineData(typeof(FormatException))]
        public async Task GetAllFiles_ThrowsExceptions(Type ex)
        {
            var cacheHandlerMock = new Mock<ICacheHandler<FileModel>>();
            var userInfoMock = new Mock<IUserInfo>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            cacheHandlerMock.Setup(x => x.CacheAndGetRange(It.IsAny<FileRangeObject>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(ex));

            var fileController = new FileController(null, null, userInfoMock.Object, cacheHandlerMock.Object);
            var result = await fileController.GetAllFiles(0, 5, true, string.Empty, string.Empty, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
