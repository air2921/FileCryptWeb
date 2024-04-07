using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Admin;
using webapi.DB.Abstractions;
using webapi.DB.Ef.Specifications.Sorting_Specifications;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Models;

namespace tests.Controllers_Tests.Admin
{
    public class MimeController_Test
    {
        [Fact]
        public async Task CreateNewMime_Success()
        {
            var mime = "hi";

            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            var redisCacheMock = new Mock<IRedisCache>();
            var loggerMock = new FakeLogger<Admin_MimeController>();

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, loggerMock, redisCacheMock.Object, null);
            var result = await mimeController.CreateNewMime(mime);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(201, objectResult.StatusCode);
            mimeRepositoryMock.Verify(x => x.Add(It.Is<FileMimeModel>(m => m.mime_name == mime), null, CancellationToken.None), Times.Once);
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

            mimeRepositoryMock.Setup(x => x.GetAll(null, CancellationToken.None)).ReturnsAsync(new List<FileMimeModel>());

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, null, redisCacheMock.Object, fileManagerMock.Object);
            var result = await mimeController.CreateMIMICollection();

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(201, objectResult.StatusCode);
            mimeRepositoryMock.Verify(x => x.AddRange(It.IsAny<IEnumerable<FileMimeModel>>(), CancellationToken.None), Times.Once);
            redisCacheMock.Verify(cache => cache.DeleteCache(ImmutableData.MIME_COLLECTION), Times.Once);
        }

        [Fact]
        public async Task CreateMIMICollection_DbConnectionFailed()
        {
            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            var fileManagerMock = new Mock<IFileManager>();
            var redisCacheMock = new Mock<IRedisCache>();

            mimeRepositoryMock.Setup(x => x.GetAll(null, CancellationToken.None))
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

            mimeRepositoryMock.Setup(x => x.GetAll(null, CancellationToken.None)).ReturnsAsync(new List<FileMimeModel>());
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
            var id = 1;

            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            mimeRepositoryMock.Setup(x => x.GetById(id, CancellationToken.None))
                .ReturnsAsync(new FileMimeModel());

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, null, null, null);
            var result = await mimeController.GetMime(id);

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

        [Fact]
        public async Task GetMimes_Success()
        {
            var skip = 0;
            var take = 100;

            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            mimeRepositoryMock.Setup(x => x.GetAll(new MimesSortSpec(skip, take), CancellationToken.None))
                .ReturnsAsync(new List<FileMimeModel>());

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, null, null, null);
            var result = await mimeController.GetMimes(skip, take);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetMimes_DbConnectionFailed()
        {
            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            mimeRepositoryMock.Setup(x => x.GetAll(It.IsAny<MimesSortSpec>(), CancellationToken.None))
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
            var id = 1;

            var mimeRepositoryMock = new Mock<IRepository<FileMimeModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, null, redisCacheMock.Object, null);
            var result = await mimeController.DeleteMime(id);

            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
            mimeRepositoryMock.Verify(x => x.Delete(id, CancellationToken.None), Times.Once);
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
            var ids = new List<int> { 1, 2, 3 };

            var mimeController = new Admin_MimeController(mimeRepositoryMock.Object, null, redisCacheMock.Object, null);
            var result = await mimeController.DeleteMimes(ids);

            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
            mimeRepositoryMock.Verify(x => x.DeleteMany(ids, CancellationToken.None), Times.Once);
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
