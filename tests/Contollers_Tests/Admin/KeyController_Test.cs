using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Admin;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Models;

namespace tests.Contollers_Tests.Admin
{
    public class KeyController_Test
    {
        [Fact]
        public async Task GetAllKeys_Success()
        {
            var keyRepositoryMock = new Mock<IRepository<KeyModel>>();
            var keyServiceMock = new Mock<IApiAdminKeysService>();

            keyRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<KeyModel>, IQueryable<KeyModel>>>(), CancellationToken.None))
                .ReturnsAsync(new KeyModel());
            keyServiceMock.Setup(x => x.GetKeys(It.IsAny<KeyModel>())).ReturnsAsync(new HashSet<string>());

            var keyController = new Admin_KeyController(keyServiceMock.Object, keyRepositoryMock.Object);
            var result = await keyController.GetAllKeys(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetAllKeys_KeysIsNull()
        {
            var keyRepositoryMock = new Mock<IRepository<KeyModel>>();
            keyRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<KeyModel>, IQueryable<KeyModel>>>(), CancellationToken.None))
                .ReturnsAsync((KeyModel)null);

            var keyController = new Admin_KeyController(null, keyRepositoryMock.Object);
            var result = await keyController.GetAllKeys(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetAllKeys_DbConnectionFailed()
        {
            var keyRepositoryMock = new Mock<IRepository<KeyModel>>();
            keyRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<KeyModel>, IQueryable<KeyModel>>>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));

            var keyController = new Admin_KeyController(null, keyRepositoryMock.Object);
            var result = await keyController.GetAllKeys(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task RevokeReceivedKey_Success()
        {
            var keyRepositoryMock = new Mock<IRepository<KeyModel>>();
            var keyServiceMock = new Mock<IApiAdminKeysService>();

            keyRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<KeyModel>, IQueryable<KeyModel>>>(), CancellationToken.None))
                .ReturnsAsync(new KeyModel());

            var keyController = new Admin_KeyController(keyServiceMock.Object, keyRepositoryMock.Object);
            var result = await keyController.RevokeReceivedKey(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task RevokeReceivedKey_KeysInNull()
        {
            var keyRepositoryMock = new Mock<IRepository<KeyModel>>();
            keyRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<KeyModel>, IQueryable<KeyModel>>>(), CancellationToken.None))
                .ReturnsAsync((KeyModel)null);

            var keyController = new Admin_KeyController(null, keyRepositoryMock.Object);
            var result = await keyController.RevokeReceivedKey(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task RevokeReceivedKey_KeyNotRevoked()
        {
            var keyRepositoryMock = new Mock<IRepository<KeyModel>>();
            var keyServiceMock = new Mock<IApiAdminKeysService>();

            keyRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<KeyModel>, IQueryable<KeyModel>>>(), CancellationToken.None))
                .ReturnsAsync(new KeyModel());
            keyServiceMock.Setup(x => x.UpdateKey(It.IsAny<KeyModel>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(EntityNotUpdatedException)));

            var keyController = new Admin_KeyController(keyServiceMock.Object, keyRepositoryMock.Object);
            var result = await keyController.RevokeReceivedKey(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
