using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Admin;
using webapi.DB;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Models;
using webapi.Services.Core.Data_Handlers;

namespace tests.Controllers_Tests.Admin
{
    public class NotificationController_Test
    {
        [Fact]
        public async Task GetNotification_Success()
        {
            var id = 1;

            var notificationRepositoryMock = new Mock<IRepository<NotificationModel>>();
            notificationRepositoryMock.Setup(x => x.GetById(id, CancellationToken.None))
                .ReturnsAsync(new NotificationModel());

            var notificationController = new Admin_NotificationController(notificationRepositoryMock.Object, null, null);
            var result = await notificationController.GetNotification(id);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetNotification_NotificationIsNull()
        {
            var notificationRepositoryMock = new Mock<IRepository<NotificationModel>>();
            notificationRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ReturnsAsync((NotificationModel)null);

            var notificationController = new Admin_NotificationController(notificationRepositoryMock.Object, null, null);
            var result = await notificationController.GetNotification(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetNotification_DbConnectionFailed()
        {
            var notificationRepositoryMock = new Mock<IRepository<NotificationModel>>();
            notificationRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            var notificationController = new Admin_NotificationController(notificationRepositoryMock.Object, null, null);
            var result = await notificationController.GetNotification(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetRangeNotification_Success()
        {
            var notificationRepositoryMock = new Mock<IRepository<NotificationModel>>();
            var sortMock = new Mock<ISorting>();
            notificationRepositoryMock.Setup(x => x.GetAll(It.IsAny<Func<IQueryable<NotificationModel>, IQueryable<NotificationModel>>>(),
                CancellationToken.None))
                .ReturnsAsync(new List<NotificationModel>());

            var notificationController = new Admin_NotificationController(notificationRepositoryMock.Object, null, sortMock.Object);
            var result = await notificationController.GetRangeNotification(1, null, null, true);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetRangeNotification_DbConnectionFailed()
        {
            var notificationRepositoryMock = new Mock<IRepository<NotificationModel>>();
            var sortMock = new Mock<ISorting>();
            notificationRepositoryMock.Setup(x => x.GetAll(It.IsAny<Func<IQueryable<NotificationModel>, IQueryable<NotificationModel>>>(),
                CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            var notificationController = new Admin_NotificationController(notificationRepositoryMock.Object, null, sortMock.Object);
            var result = await notificationController.GetRangeNotification(1, null, null, true);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteNotification_Success()
        {
            var id = 1;

            var notificationRepositoryMock = new Mock<IRepository<NotificationModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            notificationRepositoryMock.Setup(x => x.Delete(It.IsAny<int>(), CancellationToken.None))
                .ReturnsAsync(new NotificationModel { user_id = id });
            redisCacheMock.Setup(x => x.DeteteCacheByKeyPattern(It.IsAny<string>())).Returns(Task.CompletedTask);

            var notificationController = new Admin_NotificationController(notificationRepositoryMock.Object, redisCacheMock.Object, null);
            var result = await notificationController.DeleteNotification(2);

            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
            redisCacheMock.Verify(cache => cache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{id}"), Times.Once);
        }

        [Fact]
        public async Task DeleteNotification_NotFound()
        {
            var notificationRepositoryMock = new Mock<IRepository<NotificationModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            notificationRepositoryMock.Setup(x => x.Delete(It.IsAny<int>(), CancellationToken.None))
                .ReturnsAsync((NotificationModel)null);

            var notificationController = new Admin_NotificationController(notificationRepositoryMock.Object, redisCacheMock.Object, null);
            var result = await notificationController.DeleteNotification(1);

            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
            redisCacheMock.Verify(cache => cache.DeteteCacheByKeyPattern(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteNotification_EntityNotDeleted()
        {
            var notificationRepositoryMock = new Mock<IRepository<NotificationModel>>();
            notificationRepositoryMock.Setup(x => x.Delete(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new EntityNotDeletedException());

            var notificationController = new Admin_NotificationController(notificationRepositoryMock.Object, null, null);
            var result = await notificationController.DeleteNotification(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteRangeNotifications_Success()
        {
            var ids = new List<int> { 1, 2, 3 };

            var notificationRepositoryMock = new Mock<IRepository<NotificationModel>>();
            var redisCacheMock = new Mock<IRedisCache>();
            notificationRepositoryMock.Setup(x => x.DeleteMany(ids, CancellationToken.None))
                .ReturnsAsync(new List<NotificationModel>());

            var notificationController = new Admin_NotificationController(notificationRepositoryMock.Object, redisCacheMock.Object, null);
            var result = await notificationController.DeleteRangeNotifications(ids);

            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
            redisCacheMock.Verify(cache => cache.DeleteRedisCache(It.IsAny<IEnumerable<NotificationModel>>(),
                It.IsAny<string>(), It.IsAny<Func<NotificationModel, int>>()), Times.Once);
        }

        [Fact]
        public async Task DeleteRangeNotifications_EntityNotDeleted()
        {
            var notificationRepositoryMock = new Mock<IRepository<NotificationModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            notificationRepositoryMock.Setup(x => x.DeleteMany(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
                .ThrowsAsync(new EntityNotDeletedException());

            var notificationController = new Admin_NotificationController(notificationRepositoryMock.Object, redisCacheMock.Object, null);
            var result = await notificationController.DeleteRangeNotifications(new List<int> { 1 });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
            redisCacheMock.Verify(cache => cache.DeleteRedisCache(It.IsAny<IEnumerable<NotificationModel>>(),
                It.IsAny<string>(), It.IsAny<Func<NotificationModel, int>>()), Times.Never);
        }
    }
}
