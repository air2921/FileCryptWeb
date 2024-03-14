using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Admin;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;

namespace tests.Controllers_Tests.Admin
{
    public class AppController_Test
    {
        [Fact]
        public async Task FreezeService_SuccessFreezed()
        {
            var redisCacheMock = new Mock<IRedisCache>();
            var userInfoMock = new Mock<IUserInfo>();
            var logger = new FakeLogger<AppController>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            userInfoMock.Setup(x => x.Username).Returns("username");

            var appController = new AppController(userInfoMock.Object, logger, redisCacheMock.Object);
            var result = await appController.FreezeService(true, TimeSpan.FromDays(1));

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Single(logger.LoggedMessages);
        }

        [Fact]
        public async Task FreezeService_SuccessUnfreezed()
        {
            var redisCacheMock = new Mock<IRedisCache>();
            var userInfoMock = new Mock<IUserInfo>();
            var logger = new FakeLogger<AppController>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            userInfoMock.Setup(x => x.Username).Returns("username");

            var appController = new AppController(userInfoMock.Object, logger, redisCacheMock.Object);
            var result = await appController.FreezeService(false, null);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Single(logger.LoggedMessages);
        }
    }
}
