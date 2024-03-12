using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Admin;
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

        }
    }
}
