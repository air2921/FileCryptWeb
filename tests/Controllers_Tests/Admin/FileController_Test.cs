using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Admin;
using webapi.DB;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Models;

namespace tests.Controllers_Tests.Admin
{
    public class FileController_Test
    {
        [Fact]
        public async Task GetFile_Success()
        {
            var id = 1;

            var fileRepositoryMock = new Mock<IRepository<FileModel>>();

            fileRepositoryMock.Setup(x => x.GetById(id, CancellationToken.None))
                .ReturnsAsync(new FileModel());

            var fileController = new Admin_FileController(fileRepositoryMock.Object, null, null);
            var result = await fileController.GetFile(id);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetFile_FileIsNull()
        {
            var fileRepositoryMock = new Mock<IRepository<FileModel>>();

            fileRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ReturnsAsync((FileModel)null);

            var fileController = new Admin_FileController(fileRepositoryMock.Object, null, null);
            var result = await fileController.GetFile(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetFile_DbConnectionFailed()
        {
            var fileRepositoryMock = new Mock<IRepository<FileModel>>();

            fileRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            var fileController = new Admin_FileController(fileRepositoryMock.Object, null, null);
            var result = await fileController.GetFile(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetFiles_Success()
        {
            var fileRepositoryMock = new Mock<IRepository<FileModel>>();
            var sortMock = new Mock<ISorting>();

            fileRepositoryMock.Setup(x => x.GetAll(It.IsAny<Func<IQueryable<FileModel>, IQueryable<FileModel>>>(), CancellationToken.None))
                .ReturnsAsync(new List<FileModel>());

            var fileController = new Admin_FileController(fileRepositoryMock.Object, sortMock.Object, null);
            var result = await fileController.GetFiles(null, null, null, true, null);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetFiles_DbConnectionFailed()
        {
            var fileRepositoryMock = new Mock<IRepository<FileModel>>();
            var sortMock = new Mock<ISorting>();

            fileRepositoryMock.Setup(x => x.GetAll(It.IsAny<Func<IQueryable<FileModel>, IQueryable<FileModel>>>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            var fileController = new Admin_FileController(fileRepositoryMock.Object, sortMock.Object, null);
            var result = await fileController.GetFiles(null, null, null, true, null);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteFile_Success()
        {
            var id = 1;

            var fileRepositoryMock = new Mock<IRepository<FileModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            fileRepositoryMock.Setup(x => x.Delete(It.IsAny<int>(), CancellationToken.None))
                .ReturnsAsync(new FileModel { user_id = id });

            var fileController = new Admin_FileController(fileRepositoryMock.Object, null, redisCacheMock.Object);
            var result = await fileController.DeleteFile(1);

            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
            redisCacheMock.Verify(cache => cache.DeteteCacheByKeyPattern($"{ImmutableData.FILES_PREFIX}{id}"), Times.Once);
        }

        [Fact]
        public async Task DeleteFile_EntityNotDeleted()
        {
            var fileRepositoryMock = new Mock<IRepository<FileModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            fileRepositoryMock.Setup(x => x.Delete(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new EntityNotDeletedException());

            var fileController = new Admin_FileController(fileRepositoryMock.Object, null, redisCacheMock.Object);
            var result = await fileController.DeleteFile(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
            redisCacheMock.Verify(cache => cache.DeteteCacheByKeyPattern(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteFiles_Success()
        {
            var id = 1;

            var fileRepositoryMock = new Mock<IRepository<FileModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            fileRepositoryMock.Setup(x => x.DeleteMany(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
                .ReturnsAsync(new List<FileModel> { new FileModel { user_id = id } });

            var fileController = new Admin_FileController(fileRepositoryMock.Object, null, redisCacheMock.Object);
            var result = await fileController.DeleteRangeFiles(new List<int> { 1 });

            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
            redisCacheMock.Verify(cache => cache.DeleteRedisCache(It.IsAny<IEnumerable<FileModel>>(), ImmutableData.FILES_PREFIX, It.IsAny<Func<FileModel, int>>()), Times.Once);
        }

        [Fact]
        public async Task DeleteFiles_EntityNotDeleted()
        {
            var fileRepositoryMock = new Mock<IRepository<FileModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            fileRepositoryMock.Setup(x => x.DeleteMany(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
                .ThrowsAsync(new EntityNotDeletedException());

            var fileController = new Admin_FileController(fileRepositoryMock.Object, null, redisCacheMock.Object);
            var result = await fileController.DeleteRangeFiles(new List<int> { 1 });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
            redisCacheMock.Verify(cache => cache.DeleteRedisCache(It.IsAny<IEnumerable<FileModel>>(), It.IsAny<string>(), It.IsAny<Func<FileModel, int>>()), Times.Never);
        }
    }
}
