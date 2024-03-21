using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Account.Edit;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Services;
using webapi.Models;

namespace tests.Controllers_Tests.Account.Edit
{
    public class PasswordController_Test
    {
        [Fact]
        public async Task UpdatePassword_Success()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var passwordServiceMock = new Mock<IPasswordService>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel
            {
                id = 1,
                password = "test"
            });
            userInfoMock.Setup(x => x.UserId).Returns(1);
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            passwordServiceMock.Setup(x => x.UpdateTransaction(It.IsAny<UserModel>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            passwordServiceMock.Setup(x => x.ClearData(It.IsAny<int>())).Returns(Task.CompletedTask);
            passwordServiceMock.Setup(x => x.ValidatePassword(It.IsAny<string>())).Returns(true);

            var passwordController = new PasswordController(passwordServiceMock.Object, userRepositoryMock.Object,
                null, passwordManagerMock.Object, userInfoMock.Object);

            var result = await passwordController.UpdatePassword(new PasswordDTO { NewPassword = string.Empty, OldPassword = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdatePassword_NewPassword_InvalidFormat()
        {
            var passwordServiceMock = new Mock<IPasswordService>();
            passwordServiceMock.Setup(x => x.ValidatePassword(It.IsAny<string>())).Returns(false);

            var passwordController = new PasswordController(passwordServiceMock.Object, null, null, null, null);

            var result = await passwordController.UpdatePassword(new PasswordDTO { NewPassword = string.Empty, OldPassword = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(422, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdatePassword_UserNotFound()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var passwordServiceMock = new Mock<IPasswordService>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);
            userInfoMock.Setup(x => x.UserId).Returns(1);
            passwordServiceMock.Setup(x => x.ValidatePassword(It.IsAny<string>())).Returns(true);

            var passwordController = new PasswordController(passwordServiceMock.Object, userRepositoryMock.Object, null, null, userInfoMock.Object);

            var result = await passwordController.UpdatePassword(new PasswordDTO { NewPassword = string.Empty, OldPassword = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdatePassword_DbConnectionFailed_ThrowsException()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var passwordServiceMock = new Mock<IPasswordService>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));
            userInfoMock.Setup(x => x.UserId).Returns(1);
            passwordServiceMock.Setup(x => x.ValidatePassword(It.IsAny<string>())).Returns(true);

            var passwordController = new PasswordController(passwordServiceMock.Object, userRepositoryMock.Object, null, null, userInfoMock.Object);

            var result = await passwordController.UpdatePassword(new PasswordDTO { NewPassword = string.Empty, OldPassword = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdatePassword_PasswordIncorrect()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var passwordServiceMock = new Mock<IPasswordService>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ReturnsAsync(new UserModel
                {
                    id = 1,
                    password = "test123"
                });
            userInfoMock.Setup(x => x.UserId).Returns(1);
            passwordServiceMock.Setup(x => x.ValidatePassword(It.IsAny<string>())).Returns(true);
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var passwordController = new PasswordController(passwordServiceMock.Object, userRepositoryMock.Object, null, passwordManagerMock.Object, userInfoMock.Object);

            var result = await passwordController.UpdatePassword(new PasswordDTO { NewPassword = string.Empty, OldPassword = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(401, objectResult.StatusCode);
        }

        [Theory]
        [InlineData(typeof(EntityNotCreatedException))]
        [InlineData(typeof(EntityNotUpdatedException))]
        public async Task UpdatePassword_DbTransaction_ThrowsException(Type ex)
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var passwordServiceMock = new Mock<IPasswordService>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel
            {
                id = 1,
                password = "test"
            });
            userInfoMock.Setup(x => x.UserId).Returns(1);
            passwordServiceMock.Setup(x => x.ValidatePassword(It.IsAny<string>())).Returns(true);
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            passwordServiceMock.Setup(x => x.UpdateTransaction(It.IsAny<UserModel>(), It.IsAny<string>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(ex));

            var passwordController = new PasswordController(passwordServiceMock.Object, userRepositoryMock.Object,
                null, passwordManagerMock.Object, userInfoMock.Object);

            var result = await passwordController.UpdatePassword(new PasswordDTO { NewPassword = string.Empty, OldPassword = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
