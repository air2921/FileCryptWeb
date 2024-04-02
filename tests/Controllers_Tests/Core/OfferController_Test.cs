using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using webapi.Controllers.Core;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Services;
using webapi.Models;
using webapi.Services.Core;

namespace tests.Controllers_Tests.Core
{
    public class OfferController_Test
    {
        [Fact]
        public async Task CreateOneOffer_Success()
        {
            var userId = 1;
            var receiverId = 2;
            var key = "key";

            var userInfoMock = new Mock<IUserInfo>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var keyRepositoryMock = new Mock<IRepository<KeyModel>>();
            var transactionMock = new Mock<ITransaction<Participants>>();
            var dataManagementMock = new Mock<IDataManagement>();

            userInfoMock.Setup(x => x.UserId).Returns(userId);
            userRepositoryMock.Setup(x => x.GetById(receiverId, CancellationToken.None)).ReturnsAsync(new UserModel{ id = receiverId });
            keyRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<KeyModel>, IQueryable<KeyModel>>>(), CancellationToken.None))
                .ReturnsAsync(new KeyModel { internal_key = key });

            var offerController = new OfferController(dataManagementMock.Object, null, transactionMock.Object, userRepositoryMock.Object,
                null, keyRepositoryMock.Object, null, null, userInfoMock.Object);

            var result = await offerController.CreateOneOffer(receiverId);

            transactionMock.Verify(x => x.CreateTransaction(new Participants(userId, receiverId), key), Times.Once);
            dataManagementMock.Verify(x => x.DeleteData(userId, receiverId), Times.Once);
            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(201, objectResult.StatusCode);
        }
    }
}
