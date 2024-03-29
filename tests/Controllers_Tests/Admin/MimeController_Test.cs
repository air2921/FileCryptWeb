using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Admin;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Models;

namespace tests.Controllers_Tests.Admin
{
    public class MimeController_Test
    {
        [Fact]
        public async Task CreateNewMime_Success()
        {
            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            var redisCacheMock = new Mock<IRedisCache>();
            var loggerMock = new FakeLogger<Admin_MimeController>();

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, loggerMock, redisCacheMock.Object, null);
            var result = await mimeController.CreateNewMime("hi");

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(201, objectResult.StatusCode);
            redisCacheMock.Verify(cache => cache.DeleteCache(ImmutableData.MIME_COLLECTION), Times.Once);
        }

        [Fact]
        public async Task CreateNewMime_EntityNotCreated()
        {
            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            var redisCacheMock = new Mock<IRedisCache>();
            var loggerMock = new FakeLogger<Admin_MimeController>();

            mimeRepositoryMock.Setup(x => x.Add(It.IsAny<FileMimeModel>(), null, CancellationToken.None))
                .ThrowsAsync(new EntityNotCreatedException());

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, loggerMock, redisCacheMock.Object, null);
            var result = await mimeController.CreateNewMime("hi");

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateMIMICollection_Success()
        {
            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            var fileManagerMock = new Mock<IFileManager>();
            var redisCacheMock = new Mock<IRedisCache>();

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, null, redisCacheMock.Object, fileManagerMock.Object);
            var result = await mimeController.CreateMIMICollection();

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(201, objectResult.StatusCode);
            redisCacheMock.Verify(cache => cache.DeleteCache(ImmutableData.MIME_COLLECTION), Times.Once);
        }

        [Fact]
        public async Task CreateMIMICollection_DbConnectionFailed()
        {
            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            var fileManagerMock = new Mock<IFileManager>();
            var redisCacheMock = new Mock<IRedisCache>();

            mimeRepositoryMock.Setup(x => x.GetAll(It.IsAny<Func<IQueryable<FileMimeModel>, IQueryable<FileMimeModel>>>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, null, redisCacheMock.Object, fileManagerMock.Object);
            var result = await mimeController.CreateMIMICollection();

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateMIMICollection_EntityNotCreated()
        {
            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            var fileManagerMock = new Mock<IFileManager>();
            var redisCacheMock = new Mock<IRedisCache>();

            mimeRepositoryMock.Setup(x => x.AddRange(It.IsAny<IEnumerable<FileMimeModel>>(), CancellationToken.None))
                .ThrowsAsync(new EntityNotCreatedException());

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, null, redisCacheMock.Object, fileManagerMock.Object);
            var result = await mimeController.CreateMIMICollection();

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetMime_Success()
        {
            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            mimeRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ReturnsAsync(new FileMimeModel());

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, null, null, null);
            var result = await mimeController.GetMime(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetMime_MimeIsNull()
        {
            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            mimeRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ReturnsAsync((FileMimeModel)null);

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, null, null, null);
            var result = await mimeController.GetMime(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetMime_DbConnectionFailed()
        {
            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            mimeRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, null, null, null);
            var result = await mimeController.GetMime(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(0, 100)]
        public async Task GetMimes_Success(int? skip, int? take)
        {
            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            mimeRepositoryMock.Setup(x => x.GetAll(It.IsAny<Func<IQueryable<FileMimeModel>, IQueryable<FileMimeModel>>>(), CancellationToken.None))
                .ReturnsAsync(new List<FileMimeModel>());

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, null, null, null);
            var result = await mimeController.GetMimes(skip, take);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Theory]
        [InlineData(1, null)]
        [InlineData(null, 1)]
        public async Task GetMimes_InvalidParameters(int? skip, int? take)
        {
            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            mimeRepositoryMock.Setup(x => x.GetAll(It.IsAny<Func<IQueryable<FileMimeModel>, IQueryable<FileMimeModel>>>(), CancellationToken.None))
                .ReturnsAsync(new List<FileMimeModel>());

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, null, null, null);
            var result = await mimeController.GetMimes(skip, take);

            Assert.Equal(400, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task GetMimes_DbConnectionFailed()
        {
            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            mimeRepositoryMock.Setup(x => x.GetAll(It.IsAny<Func<IQueryable<FileMimeModel>, IQueryable<FileMimeModel>>>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, null, null, null);
            var result = await mimeController.GetMimes(1, 1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteMime_Success()
        {
            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, null, redisCacheMock.Object, null);
            var result = await mimeController.DeleteMime(1);

            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
            redisCacheMock.Verify(cache => cache.DeleteCache(ImmutableData.MIME_COLLECTION), Times.Once);
        }

        [Fact]
        public async Task DeleteMime_EntityNotDeleted()
        {
            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            mimeRepositoryMock.Setup(x => x.Delete(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new EntityNotDeletedException());

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, null, null, null);
            var result = await mimeController.DeleteMime(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteMimes_Success()
        {
            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, null, redisCacheMock.Object, null);
            var result = await mimeController.DeleteMimes(new List<int> { 1 });

            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
            redisCacheMock.Verify(cache => cache.DeleteCache(ImmutableData.MIME_COLLECTION), Times.Once);
        }

        [Fact]
        public async Task DeleteMimes_EntityNotDeleted()
        {
            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            mimeRepositoryMock.Setup(x => x.DeleteMany(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
                .ThrowsAsync(new EntityNotDeletedException());

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, null, null, null);
            var result = await mimeController.DeleteMimes(new List<int> { 1 });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
