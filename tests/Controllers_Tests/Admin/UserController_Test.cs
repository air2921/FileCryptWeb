using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Admin;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Models;

namespace tests.Controllers_Tests.Admin
{
    public class UserController_Test
    {
        [Fact]
        public async Task GetUser_Success()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var userController = new Admin_UserController(null, null, userRepositoryMock.Object);
            var result = await userController.GetUser(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetUser_UserNotFound()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);

            var userController = new Admin_UserController(null, null, userRepositoryMock.Object);
            var result = await userController.GetUser(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetUser_DbConnectionFailed()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            var userController = new Admin_UserController(null, null, userRepositoryMock.Object);
            var result = await userController.GetUser(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_Success()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var validatorMock = new Mock<IValidator>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var userController = new Admin_UserController(null, validatorMock.Object, userRepositoryMock.Object);
            var result = await userController.DeleteUser(1);

            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task DeleteUser_UserNotFound()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);

            var userController = new Admin_UserController(null, null, userRepositoryMock.Object);
            var result = await userController.DeleteUser(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_DbConnectionFailed()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            var userController = new Admin_UserController(null, null, userRepositoryMock.Object);
            var result = await userController.DeleteUser(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_TargetIsHighestAdmin()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var validatorMock = new Mock<IValidator>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(false);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var userController = new Admin_UserController(null, validatorMock.Object, userRepositoryMock.Object);
            var result = await userController.DeleteUser(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(403, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_EntityNotDeleted()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var validatorMock = new Mock<IValidator>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);
            userRepositoryMock.Setup(x => x.Delete(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new EntityNotDeletedException());
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var userController = new Admin_UserController(null, validatorMock.Object, userRepositoryMock.Object);
            var result = await userController.DeleteUser(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task BlockUser_Success()
        {
            var validatorMock = new Mock<IValidator>();
            var transactionMock = new Mock<ITransaction<UserModel>>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var userController = new Admin_UserController(transactionMock.Object, validatorMock.Object, userRepositoryMock.Object);
            var result = await userController.BlockUser(1, true);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task BlockUser_UserNotFound()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);

            var userController = new Admin_UserController(null, null, userRepositoryMock.Object);
            var result = await userController.BlockUser(1, true);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task BlockUser_DbConnectionFailed()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            var userController = new Admin_UserController(null, null, userRepositoryMock.Object);
            var result = await userController.BlockUser(1, true);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task BlockUser_Forbidden()
        {
            var validatorMock = new Mock<IValidator>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(false);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var userController = new Admin_UserController(null, validatorMock.Object, userRepositoryMock.Object);
            var result = await userController.BlockUser(1, true);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(403, objectResult.StatusCode);
        }

        [Theory]
        [InlineData(typeof(EntityNotDeletedException))]
        [InlineData(typeof(EntityNotUpdatedException))]
        public async Task BlockUser_DbTransaction_ThrowsEx(Type ex)
        {
            var validatorMock = new Mock<IValidator>();
            var transactionMock = new Mock<ITransaction<UserModel>>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);
            transactionMock.Setup(x => x.CreateTransaction(It.IsAny<UserModel>(), It.IsAny<bool>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(ex));
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var userController = new Admin_UserController(transactionMock.Object, validatorMock.Object, userRepositoryMock.Object);
            var result = await userController.BlockUser(1, true);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateRole_Success()
        {
            var validatorMock = new Mock<IValidator>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var userController = new Admin_UserController(null, validatorMock.Object, userRepositoryMock.Object);
            var result = await userController.UpdateRole(1, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }


        [Fact]
        public async Task UpdateRole_Forbidden()
        {
            var validatorMock = new Mock<IValidator>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(false);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var userController = new Admin_UserController(null, validatorMock.Object, userRepositoryMock.Object);
            var result = await userController.UpdateRole(1, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(403, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateRole_UserNotFound()
        {
            var validatorMock = new Mock<IValidator>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);

            var userController = new Admin_UserController(null, validatorMock.Object, userRepositoryMock.Object);
            var result = await userController.UpdateRole(1, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateRole_DbConnectionFailed()
        {
            var validatorMock = new Mock<IValidator>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            var userController = new Admin_UserController(null, validatorMock.Object, userRepositoryMock.Object);
            var result = await userController.UpdateRole(1, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateRole_EntityNotUpdated()
        {
            var validatorMock = new Mock<IValidator>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());
            userRepositoryMock.Setup(x => x.Update(It.IsAny<UserModel>(), CancellationToken.None))
                .ThrowsAsync(new EntityNotUpdatedException());

            var userController = new Admin_UserController(null, validatorMock.Object, userRepositoryMock.Object);
            var result = await userController.UpdateRole(1, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
