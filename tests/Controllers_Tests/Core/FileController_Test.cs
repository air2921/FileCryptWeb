using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Core;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Models;

namespace tests.Controllers_Tests.Core
{
    public class FileController_Test
    {
        [Fact]
        public async Task DeleteFileFromHistory_Success()
        {
            var fileRepositoryMock = new Mock<IRepository<FileModel>>();
            var redisCacheMock = new Mock<IRedisCache>();
            var userInfoMock = new Mock<IUserInfo>();

            userInfoMock.Setup(x => x.UserId).Returns(1);

            var fileController = new FileController(fileRepositoryMock.Object, redisCacheMock.Object, userInfoMock.Object,
                null);

            var result = await fileController.DeleteFileFromHistory(1);

            fileRepositoryMock.Verify(repo => repo
                .DeleteByFilter(It.IsAny<Func<IQueryable<FileModel>, IQueryable<FileModel>>>(), CancellationToken.None), Times.Once);
            redisCacheMock.Verify(cache => cache.DeteteCacheByKeyPattern(It.IsAny<string>()), Times.Once);
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
    }
}
