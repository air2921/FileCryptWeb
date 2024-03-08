using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Account.Edit;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Services;
using webapi.Models;

namespace tests.Contollers_Tests.Account.Edit
{
    public class UsernameController_Test
    {
        [Fact]
        public async Task UpdateUsername_Success()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var tokenServiceMock = new Mock<ITokenService>();
            var usernameServiceMock = new Mock<IApiUsernameService>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());
            userInfoMock.Setup(x => x.UserId).Returns(1);
            usernameServiceMock.Setup(x => x.ValidateUsername(It.IsAny<string>())).Returns(true);

            var usernameController = new UsernameController(userRepositoryMock.Object, usernameServiceMock.Object,
                null, userInfoMock.Object, tokenServiceMock.Object);

            var result = await usernameController.UpdateUsername(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateUsername_InvalidUsername()
        {
            var usernameServiceMock = new Mock<IApiUsernameService>();

            usernameServiceMock.Setup(x => x.ValidateUsername(It.IsAny<string>())).Returns(false);

            var usernameController = new UsernameController(null, usernameServiceMock.Object, null, null, null);

            var result = await usernameController.UpdateUsername(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateUsername_UserNotFound()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var usernameServiceMock = new Mock<IApiUsernameService>();
            var userInfoMock = new Mock<IUserInfo>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);
            userInfoMock.Setup(x => x.UserId).Returns(1);
            usernameServiceMock.Setup(x => x.ValidateUsername(It.IsAny<string>())).Returns(true);

            var usernameController = new UsernameController(userRepositoryMock.Object, usernameServiceMock.Object,
                null, userInfoMock.Object, null);

            var result = await usernameController.UpdateUsername(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateUsername_DbConnectionFailed()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var usernameServiceMock = new Mock<IApiUsernameService>();
            var userInfoMock = new Mock<IUserInfo>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));
            userInfoMock.Setup(x => x.UserId).Returns(1);
            usernameServiceMock.Setup(x => x.ValidateUsername(It.IsAny<string>())).Returns(true);

            var usernameController = new UsernameController(userRepositoryMock.Object, usernameServiceMock.Object,
                null, userInfoMock.Object, null);

            var result = await usernameController.UpdateUsername(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateUsername_UpdateFailed_ThrowsException()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var usernameServiceMock = new Mock<IApiUsernameService>();
            var userInfoMock = new Mock<IUserInfo>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());
            userInfoMock.Setup(x => x.UserId).Returns(1);
            usernameServiceMock.Setup(x => x.ValidateUsername(It.IsAny<string>())).Returns(true);
            usernameServiceMock.Setup(x => x.DbUpdate(It.IsAny<UserModel>(), It.IsAny<string>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(EntityNotUpdatedException)));

            var usernameController = new UsernameController(userRepositoryMock.Object, usernameServiceMock.Object,
                null, userInfoMock.Object, null);

            var result = await usernameController.UpdateUsername(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateUsername_UpdateSuccess_JwtNotUpdated()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var tokenServiceMock = new Mock<ITokenService>();
            var usernameServiceMock = new Mock<IApiUsernameService>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());
            userInfoMock.Setup(x => x.UserId).Returns(1);
            usernameServiceMock.Setup(x => x.ValidateUsername(It.IsAny<string>())).Returns(true);
            tokenServiceMock.Setup(x => x.UpdateJwtToken()).ThrowsAsync((Exception)Activator.CreateInstance(typeof(UnauthorizedAccessException)));

            var usernameController = new UsernameController(userRepositoryMock.Object, usernameServiceMock.Object,
                null, userInfoMock.Object, tokenServiceMock.Object);

            var result = await usernameController.UpdateUsername(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(206, objectResult.StatusCode);
        }
    }
}
