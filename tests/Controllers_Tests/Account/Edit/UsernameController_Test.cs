using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Account.Edit;
using webapi.DB.Abstractions;
using webapi.Exceptions;
using webapi.Helpers.Abstractions;
using webapi.Models;
using webapi.Services.Abstractions;

namespace tests.Controllers_Tests.Account.Edit
{
    public class UsernameController_Test
    {
        [Fact]
        public async Task UpdateUsername_Success()
        {
            var id = 1;
            var username = "air2921";
            var user = new UserModel();

            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var tokenServiceMock = new Mock<ITokenService>();
            var transactionMock = new Mock<ITransaction<UserModel>>();
            var dateManagementMock = new Mock<IDataManagement>();
            var validatorMock = new Mock<IValidator>();

            userRepositoryMock.Setup(x => x.GetById(id, CancellationToken.None)).ReturnsAsync(user);
            userInfoMock.Setup(x => x.UserId).Returns(id);
            validatorMock.Setup(x => x.IsValid(username, null)).Returns(true);

            var usernameController = new UsernameController(transactionMock.Object, dateManagementMock.Object, validatorMock.Object,
                userRepositoryMock.Object, userInfoMock.Object, tokenServiceMock.Object);

            var result = await usernameController.UpdateUsername(username);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);

            transactionMock.Verify(x => x.CreateTransaction(user, username), Times.Once);
            dateManagementMock.Verify(x => x.DeleteData(id, null), Times.Once);
            tokenServiceMock.Verify(x => x.UpdateJwtToken(), Times.Once);
        }

        [Fact]
        public async Task UpdateUsername_InvalidUsername()
        {
            var validatorMock = new Mock<IValidator>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(false);

            var usernameController = new UsernameController(null, null, validatorMock.Object, null, null, null);

            var result = await usernameController.UpdateUsername(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateUsername_UserNotFound()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var validatorMock = new Mock<IValidator>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);
            userInfoMock.Setup(x => x.UserId).Returns(1);
            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);

            var usernameController = new UsernameController(null, null, validatorMock.Object, userRepositoryMock.Object,
                userInfoMock.Object, null);

            var result = await usernameController.UpdateUsername(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateUsername_DbConnectionFailed()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var validatorMock = new Mock<IValidator>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());
            userInfoMock.Setup(x => x.UserId).Returns(1);
            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);

            var usernameController = new UsernameController(null, null, validatorMock.Object, userRepositoryMock.Object,
                userInfoMock.Object, null);

            var result = await usernameController.UpdateUsername(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateUsername_UpdateFailed_ThrowsException()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var tokenServiceMock = new Mock<ITokenService>();
            var transactionMock = new Mock<ITransaction<UserModel>>();
            var validatorMock = new Mock<IValidator>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());
            userInfoMock.Setup(x => x.UserId).Returns(1);
            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);
            transactionMock.Setup(x => x.CreateTransaction(It.IsAny<UserModel>(), It.IsAny<string>()))
                .ThrowsAsync(new EntityNotUpdatedException());

            var usernameController = new UsernameController(transactionMock.Object, null, validatorMock.Object,
                userRepositoryMock.Object, userInfoMock.Object, tokenServiceMock.Object);

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
            var transactionMock = new Mock<ITransaction<UserModel>>();
            var dateManagementMock = new Mock<IDataManagement>();
            var validatorMock = new Mock<IValidator>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());
            userInfoMock.Setup(x => x.UserId).Returns(1);
            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);
            transactionMock.Setup(x => x.CreateTransaction(It.IsAny<UserModel>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            dateManagementMock.Setup(x => x.DeleteData(It.IsAny<int>(), null)).Returns(Task.CompletedTask);
            tokenServiceMock.Setup(x => x.UpdateJwtToken())
                .ThrowsAsync(new UnauthorizedAccessException());

            var usernameController = new UsernameController(transactionMock.Object, dateManagementMock.Object, validatorMock.Object,
                userRepositoryMock.Object, userInfoMock.Object, tokenServiceMock.Object);

            var result = await usernameController.UpdateUsername(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(206, objectResult.StatusCode);

            tokenServiceMock.Verify(x => x.DeleteTokens(), Times.Once);
        }
    }
}
