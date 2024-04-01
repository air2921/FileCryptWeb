﻿using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Core;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Models;
using webapi.Services.Core.Data_Handlers;

namespace tests.Controllers_Tests.Core
{
    public class NotificationController_Test
    {
        [Fact]
        public async Task GetNotification_Success()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var cacheMock = new Mock<ICacheHandler<NotificationModel>>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            cacheMock.Setup(x => x.CacheAndGet(It.IsAny<NotificationObject>())).ReturnsAsync(new NotificationModel());

            var ntfController = new NotificationController(null, cacheMock.Object, null, userInfoMock.Object);
            var result = await ntfController.GetNotification(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetNotification_NotFound()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var cacheMock = new Mock<ICacheHandler<NotificationModel>>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            cacheMock.Setup(x => x.CacheAndGet(It.IsAny<NotificationObject>())).ReturnsAsync((NotificationModel)null);

            var ntfController = new NotificationController(null, cacheMock.Object, null, userInfoMock.Object);
            var result = await ntfController.GetNotification(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Theory]
        [InlineData(typeof(OperationCanceledException))]
        [InlineData(typeof(FormatException))]
        public async Task GetNotification_ThrowsExceptions(Type ex)
        {
            var userInfoMock = new Mock<IUserInfo>();
            var cacheMock = new Mock<ICacheHandler<NotificationModel>>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            cacheMock.Setup(x => x.CacheAndGet(It.IsAny<NotificationObject>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(ex));

            var ntfController = new NotificationController(null, cacheMock.Object, null, userInfoMock.Object);
            var result = await ntfController.GetNotification(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetAll_Success()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var cacheMock = new Mock<ICacheHandler<NotificationModel>>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            cacheMock.Setup(x => x.CacheAndGetRange(It.IsAny<NotificationRangeObject>())).ReturnsAsync(new List<NotificationModel>());

            var ntfController = new NotificationController(null, cacheMock.Object, null, userInfoMock.Object);
            var result = await ntfController.GetAll(0, 5, true, string.Empty, null);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Theory]
        [InlineData(typeof(OperationCanceledException))]
        [InlineData(typeof(FormatException))]
        public async Task GetAll_ThrowsExceptions(Type ex)
        {
            var userInfoMock = new Mock<IUserInfo>();
            var cacheMock = new Mock<ICacheHandler<NotificationModel>>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            cacheMock.Setup(x => x.CacheAndGetRange(It.IsAny<NotificationRangeObject>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(ex));

            var ntfController = new NotificationController(null, cacheMock.Object, null, userInfoMock.Object);
            var result = await ntfController.GetAll(0, 5, true, string.Empty, null);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteNotification_Success()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var ntfRepositoryMock = new Mock<IRepository<NotificationModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            ntfRepositoryMock
                .Setup(x => x.DeleteByFilter(It.IsAny<Func<IQueryable<NotificationModel>, IQueryable<NotificationModel>>>(), CancellationToken.None))
                    .ReturnsAsync(new NotificationModel());

            var ntfController = new NotificationController(ntfRepositoryMock.Object, null, redisCacheMock.Object, userInfoMock.Object);
            var result = await ntfController.DeleteNotification(1);

            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
            redisCacheMock.Verify(x => x.DeteteCacheByKeyPattern(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeleteNotification_NotFound()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var ntfRepositoryMock = new Mock<IRepository<NotificationModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            ntfRepositoryMock
                .Setup(x => x.DeleteByFilter(It.IsAny<Func<IQueryable<NotificationModel>, IQueryable<NotificationModel>>>(), CancellationToken.None))
                    .ReturnsAsync((NotificationModel)null);

            var ntfController = new NotificationController(ntfRepositoryMock.Object, null, redisCacheMock.Object, userInfoMock.Object);
            var result = await ntfController.DeleteNotification(1);

            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
            redisCacheMock.Verify(x => x.DeteteCacheByKeyPattern(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteNotification_ThrowsException()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var ntfRepositoryMock = new Mock<IRepository<NotificationModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            ntfRepositoryMock
                .Setup(x => x.DeleteByFilter(It.IsAny<Func<IQueryable<NotificationModel>, IQueryable<NotificationModel>>>(), CancellationToken.None))
                    .ThrowsAsync(new EntityNotDeletedException());

            var ntfController = new NotificationController(ntfRepositoryMock.Object, null, redisCacheMock.Object, userInfoMock.Object);
            var result = await ntfController.DeleteNotification(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
            redisCacheMock.Verify(x => x.DeteteCacheByKeyPattern(It.IsAny<string>()), Times.Never);
        }
    }
}
