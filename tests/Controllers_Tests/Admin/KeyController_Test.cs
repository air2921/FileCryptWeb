using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Admin;
using webapi.DB.Abstractions;
using webapi.DB.Ef.Specifications.By_Relation_Specifications;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Models;

namespace tests.Controllers_Tests.Admin
{
    public class KeyController_Test
    {
        [Fact]
        public async Task RevokeReceivedKey_Success()
        {
            var id = 1;

            var keyRepositoryMock = new Mock<IRepository<KeyModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            keyRepositoryMock.Setup(x => x.GetByFilter(It.Is<KeysByRelationSpec>(x => x.UserId == id), CancellationToken.None))
                .ReturnsAsync(new KeyModel());

            var keyController = new Admin_KeyController(redisCacheMock.Object, keyRepositoryMock.Object);
            var result = await keyController.RevokeReceivedKey(id);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
            redisCacheMock.Verify(cache => cache.DeleteCache("receivedKey#" + id), Times.Once);
            redisCacheMock.Verify(cache => cache.DeteteCacheByKeyPattern($"{ImmutableData.KEYS_PREFIX}{id}"), Times.Once);
        }

        [Fact]
        public async Task RevokeReceivedKey_KeysInNull()
        {
            var keyRepositoryMock = new Mock<IRepository<KeyModel>>();
            keyRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<KeysByRelationSpec>(), CancellationToken.None))
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
            var redisCacheMock = new Mock<IRedisCache>();

            keyRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<KeysByRelationSpec>(), CancellationToken.None))
                .ReturnsAsync(new KeyModel());
            keyRepositoryMock.Setup(x => x.Update(It.IsAny<KeyModel>(), CancellationToken.None))
                .ThrowsAsync(new EntityNotUpdatedException());

            var keyController = new Admin_KeyController(redisCacheMock.Object, keyRepositoryMock.Object);
            var result = await keyController.RevokeReceivedKey(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
            redisCacheMock.Verify(cache => cache.DeleteCache(It.IsAny<string>()), Times.Never);
            redisCacheMock.Verify(cache => cache.DeteteCacheByKeyPattern(It.IsAny<string>()), Times.Never);
        }
    }
}
