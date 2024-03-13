using Microsoft.AspNetCore.Mvc;
using Npgsql.Internal.TypeHandlers.FullTextSearchHandlers;
using Org.BouncyCastle.Tsp;
using webapi.Controllers.Admin;
using webapi.Exceptions;
using webapi.Interfaces;
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

            var userController = new Admin_UserController(null, userRepositoryMock.Object, null, null);
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

            var userController = new Admin_UserController(null, userRepositoryMock.Object, null, null);
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
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));

            var userController = new Admin_UserController(null, userRepositoryMock.Object, null, null);
            var result = await userController.GetUser(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_Success()
        {
            var userServiceMock = new Mock<IApiAdminUserService>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userServiceMock.Setup(x => x.IsHighestAdmin(It.IsAny<string>())).Returns(false);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var userController = new Admin_UserController(userServiceMock.Object, userRepositoryMock.Object, null, null);
            var result = await userController.DeleteUser(1);

            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task DeleteUser_UserNotFound()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);

            var userController = new Admin_UserController(null, userRepositoryMock.Object, null, null);
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
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));

            var userController = new Admin_UserController(null, userRepositoryMock.Object, null, null);
            var result = await userController.DeleteUser(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_TargetIsHighestAdmin()
        {
            var userServiceMock = new Mock<IApiAdminUserService>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userServiceMock.Setup(x => x.IsHighestAdmin(It.IsAny<string>())).Returns(true);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var userController = new Admin_UserController(userServiceMock.Object, userRepositoryMock.Object, null, null);
            var result = await userController.DeleteUser(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(403, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_EntityNotDeleted()
        {
            var userServiceMock = new Mock<IApiAdminUserService>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userServiceMock.Setup(x => x.IsHighestAdmin(It.IsAny<string>())).Returns(false);
            userRepositoryMock.Setup(x => x.Delete(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(EntityNotDeletedException)));
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var userController = new Admin_UserController(userServiceMock.Object, userRepositoryMock.Object, null, null);
            var result = await userController.DeleteUser(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task BlockUser_Success()
        {
            var userServiceMock = new Mock<IApiAdminUserService>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userServiceMock.Setup(x => x.IsHighestAdmin(It.IsAny<string>())).Returns(false);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var userController = new Admin_UserController(userServiceMock.Object, userRepositoryMock.Object, null, null);
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

            var userController = new Admin_UserController(null, userRepositoryMock.Object, null, null);
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
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));

            var userController = new Admin_UserController(null, userRepositoryMock.Object, null, null);
            var result = await userController.BlockUser(1, true);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task BlockUser_Forbidden()
        {
            var userServiceMock = new Mock<IApiAdminUserService>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userServiceMock.Setup(x => x.IsHighestAdmin(It.IsAny<string>())).Returns(true);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var userController = new Admin_UserController(userServiceMock.Object, userRepositoryMock.Object, null, null);
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
            var userServiceMock = new Mock<IApiAdminUserService>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userServiceMock.Setup(x => x.IsHighestAdmin(It.IsAny<string>())).Returns(false);
            userServiceMock.Setup(x => x.DbTransaction(It.IsAny<UserModel>(), It.IsAny<bool>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(ex));
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var userController = new Admin_UserController(userServiceMock.Object, userRepositoryMock.Object, null, null);
            var result = await userController.BlockUser(1, true);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateRole_Success()
        {
            var userServiceMock = new Mock<IApiAdminUserService>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userServiceMock.Setup(x => x.IsHighestAdmin(It.IsAny<string>())).Returns(false);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var userController = new Admin_UserController(userServiceMock.Object, userRepositoryMock.Object, null, null);
            var result = await userController.UpdateRole(1, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }


        [Fact]
        public async Task UpdateRole_Forbidden()
        {
            var userServiceMock = new Mock<IApiAdminUserService>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userServiceMock.Setup(x => x.IsHighestAdmin(It.IsAny<string>())).Returns(true);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var userController = new Admin_UserController(userServiceMock.Object, userRepositoryMock.Object, null, null);
            var result = await userController.UpdateRole(1, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(403, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateRole_UserNotFound()
        {
            var userServiceMock = new Mock<IApiAdminUserService>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userServiceMock.Setup(x => x.IsHighestAdmin(It.IsAny<string>())).Returns(false);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);

            var userController = new Admin_UserController(userServiceMock.Object, userRepositoryMock.Object, null, null);
            var result = await userController.UpdateRole(1, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateRole_DbConnectionFailed()
        {
            var userServiceMock = new Mock<IApiAdminUserService>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userServiceMock.Setup(x => x.IsHighestAdmin(It.IsAny<string>())).Returns(false);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));

            var userController = new Admin_UserController(userServiceMock.Object, userRepositoryMock.Object, null, null);
            var result = await userController.UpdateRole(1, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateRole_EntityNotUpdated()
        {
            var userServiceMock = new Mock<IApiAdminUserService>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userServiceMock.Setup(x => x.IsHighestAdmin(It.IsAny<string>())).Returns(false);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());
            userRepositoryMock.Setup(x => x.Update(It.IsAny<UserModel>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(EntityNotUpdatedException)));

            var userController = new Admin_UserController(userServiceMock.Object, userRepositoryMock.Object, null, null);
            var result = await userController.UpdateRole(1, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
