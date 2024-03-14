using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Admin;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Models;

namespace tests.Controllers_Tests.Admin
{
    public class ApiController_Test
    {
        [Fact]
        public async Task GetApi_ById_Success()
        {
            var apiRepositoryMock = new Mock<IRepository<ApiModel>>();

            apiRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ReturnsAsync(new ApiModel());

            var apiController = new Admin_ApiController(apiRepositoryMock.Object, null);
            var result = await apiController.GetApi(1, null);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetApi_ByKey_Success()
        {
            var apiRepositoryMock = new Mock<IRepository<ApiModel>>();

            apiRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<ApiModel>, IQueryable<ApiModel>>>(), CancellationToken.None))
                .ReturnsAsync(new ApiModel());

            var apiController = new Admin_ApiController(apiRepositoryMock.Object, null);
            var result = await apiController.GetApi(null, "key");

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetApi_ApiNull()
        {
            var apiRepositoryMock = new Mock<IRepository<ApiModel>>();

            apiRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<ApiModel>, IQueryable<ApiModel>>>(), CancellationToken.None))
                .ReturnsAsync((ApiModel)null);

            var apiController = new Admin_ApiController(apiRepositoryMock.Object, null);
            var result = await apiController.GetApi(1, null);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetApi_DbConnectionFailed()
        {
            var apiRepositoryMock = new Mock<IRepository<ApiModel>>();

            apiRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));

            var apiController = new Admin_ApiController(apiRepositoryMock.Object, null);
            var result = await apiController.GetApi(1, null);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetRangeApi_Success()
        {
            var apiRepositoryMock = new Mock<IRepository<ApiModel>>();

            apiRepositoryMock.Setup(x => x.GetAll(It.IsAny<Func<IQueryable<ApiModel>, IQueryable<ApiModel>>>(), CancellationToken.None))
                .ReturnsAsync(new List<ApiModel>());

            var apiController = new Admin_ApiController(apiRepositoryMock.Object, null);
            var result = await apiController.GetRangeApi(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetRangeApi_DbConnectionFailed()
        {
            var apiRepositoryMock = new Mock<IRepository<ApiModel>>();

            apiRepositoryMock.Setup(x => x.GetAll(It.IsAny<Func<IQueryable<ApiModel>, IQueryable<ApiModel>>>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));

            var apiController = new Admin_ApiController(apiRepositoryMock.Object, null);
            var result = await apiController.GetRangeApi(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteApi_Success()
        {
            var apiRepositoryMock = new Mock<IRepository<ApiModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            apiRepositoryMock.Setup(x => x.Delete(It.IsAny<int>(), CancellationToken.None))
                .ReturnsAsync(new ApiModel { user_id = 1 });

            var apiController = new Admin_ApiController(apiRepositoryMock.Object, redisCacheMock.Object);
            var result = await apiController.DeleteApi(1);

            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task DeleteApi_EntityNotDeleted()
        {
            var apiRepositoryMock = new Mock<IRepository<ApiModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            apiRepositoryMock.Setup(x => x.Delete(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(EntityNotDeletedException)));

            var apiController = new Admin_ApiController(apiRepositoryMock.Object, redisCacheMock.Object);
            var result = await apiController.DeleteApi(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteRangeApi_Success()
        {
            var apiRepositoryMock = new Mock<IRepository<ApiModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            apiRepositoryMock.Setup(x => x.DeleteMany(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
                .ReturnsAsync(new List<ApiModel> { new ApiModel { user_id = 1 } });

            var apiController = new Admin_ApiController(apiRepositoryMock.Object, redisCacheMock.Object);
            var result = await apiController.DeleteRangeApi(new List<int> { 1 });

            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task DeleteRangeApi_EntityNotDeleted()
        {
            var apiRepositoryMock = new Mock<IRepository<ApiModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            apiRepositoryMock.Setup(x => x.DeleteMany(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(EntityNotDeletedException)));

            var apiController = new Admin_ApiController(apiRepositoryMock.Object, redisCacheMock.Object);
            var result = await apiController.DeleteRangeApi(new List<int> { 1 });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
