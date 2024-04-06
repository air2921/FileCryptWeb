using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Core;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Services;
using webapi.Models;
using webapi.Services.Core;
using webapi.Services.Core.Data_Handlers;

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
                .ReturnsAsync(new KeyModel { internal_key = key })
                .Callback<Func<IQueryable<KeyModel>, IQueryable<KeyModel>>, CancellationToken>((query, key) => {
                    var testQuery = new List<KeyModel>().AsQueryable();
                    var filteredQuery = query(testQuery);
                    Assert.True(filteredQuery.Expression.ToString().Contains($".user_id.Equals("));
                });

            var offerController = new OfferController(dataManagementMock.Object, null, transactionMock.Object, userRepositoryMock.Object,
                null, keyRepositoryMock.Object, null, null, userInfoMock.Object);

            var result = await offerController.CreateOneOffer(receiverId);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(201, objectResult.StatusCode);
            transactionMock.Verify(x => x.CreateTransaction(new Participants(userId, receiverId), key), Times.Once);
            dataManagementMock.Verify(x => x.DeleteData(userId, receiverId), Times.Once);
        }

        [Fact]
        public async Task CreateOneOffer_SenderIsReceiver_Conflict()
        {
            var userInfoMock = new Mock<IUserInfo>();

            userInfoMock.Setup(x => x.UserId).Returns(1);

            var offerController = new OfferController(null, null, null, null,
                null, null, null, null, userInfoMock.Object);

            var result = await offerController.CreateOneOffer(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(409, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateOneOffer_ReceiverNotFound()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userInfoMock.Setup(x => x.UserId).Returns(It.IsAny<int>());
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);

            var offerController = new OfferController(null, null, null, userRepositoryMock.Object,
                null, null, null, null, userInfoMock.Object);

            var result = await offerController.CreateOneOffer(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateOneOffer_KeysIsNull()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var keyRepositoryMock = new Mock<IRepository<KeyModel>>();

            userInfoMock.Setup(x => x.UserId).Returns(It.IsAny<int>());
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());
            keyRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<KeyModel>, IQueryable<KeyModel>>>(), CancellationToken.None))
                .ReturnsAsync((KeyModel)null);

            var offerController = new OfferController(null, null, null, userRepositoryMock.Object,
                null, keyRepositoryMock.Object, null, null, userInfoMock.Object);

            var result = await offerController.CreateOneOffer(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateOneOffer_InternalKeysIsNull()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var keyRepositoryMock = new Mock<IRepository<KeyModel>>();

            userInfoMock.Setup(x => x.UserId).Returns(It.IsAny<int>());
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());
            keyRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<KeyModel>, IQueryable<KeyModel>>>(), CancellationToken.None))
                .ReturnsAsync(new KeyModel { internal_key = null })
                .Callback<Func<IQueryable<KeyModel>, IQueryable<KeyModel>>, CancellationToken>((query, key) => {
                    var testQuery = new List<KeyModel>().AsQueryable();
                    var filteredQuery = query(testQuery);
                    Assert.True(filteredQuery.Expression.ToString().Contains($".user_id.Equals("));
                });

            var offerController = new OfferController(null, null, null, userRepositoryMock.Object,
                null, keyRepositoryMock.Object, null, null, userInfoMock.Object);

            var result = await offerController.CreateOneOffer(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateOneOffer_TransactionThrowsException()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var keyRepositoryMock = new Mock<IRepository<KeyModel>>();
            var transactionMock = new Mock<ITransaction<Participants>>();

            userInfoMock.Setup(x => x.UserId).Returns(It.IsAny<int>());
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());
            keyRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<KeyModel>, IQueryable<KeyModel>>>(), CancellationToken.None))
                .ReturnsAsync(new KeyModel { internal_key = "key" });
            transactionMock.Setup(x => x.CreateTransaction(It.IsAny<Participants>(), It.IsAny<object>()))
                .ThrowsAsync(new EntityNotCreatedException());

            var offerController = new OfferController(null, null, transactionMock.Object, userRepositoryMock.Object,
                null, keyRepositoryMock.Object, null, null, userInfoMock.Object);

            var result = await offerController.CreateOneOffer(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task AcceptOffer_Success()
        {
            var receiverId = 1;
            var senderId = 2;
            var offerId = 1;
            var offer = new OfferModel
            {
                offer_id = offerId,
                sender_id = senderId,
                receiver_id = receiverId,
                is_accepted = false
            };
            var receiverKeys = new KeyModel
            {
                user_id = receiverId
            };

            var userInfoMock = new Mock<IUserInfo>();
            var offerRepositoryMock = new Mock<IRepository<OfferModel>>();
            var keyRepositoryMock = new Mock<IRepository<KeyModel>>();
            var transactionMock = new Mock<ITransaction<KeyModel>>();
            var dataManagementMock = new Mock<IDataManagement>();

            userInfoMock.Setup(x => x.UserId).Returns(receiverId);
            offerRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<OfferModel>, IQueryable<OfferModel>>>(), CancellationToken.None))
                .ReturnsAsync(offer)
                .Callback<Func<IQueryable<OfferModel>, IQueryable<OfferModel>>, CancellationToken>((query, token) => {
                    var testQuery = new List<OfferModel>().AsQueryable();
                    var filteredQuery = query(testQuery);
                    Assert.True(filteredQuery.Expression.ToString().Contains($".offer_id.Equals("));
                    Assert.True(filteredQuery.Expression.ToString().Contains($".receiver_id.Equals("));
                });
            keyRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<KeyModel>, IQueryable<KeyModel>>>(), CancellationToken.None))
                .ReturnsAsync(receiverKeys)
                .Callback<Func<IQueryable<KeyModel>, IQueryable<KeyModel>>, CancellationToken>((query, token) => {
                    var testQuery = new List<KeyModel>().AsQueryable();
                    var filteredQuery = query(testQuery);
                    Assert.True(filteredQuery.Expression.ToString().Contains($".user_id.Equals("));
                });

            var offerController = new OfferController(dataManagementMock.Object, transactionMock.Object, null, null,
                offerRepositoryMock.Object, keyRepositoryMock.Object, null, null, userInfoMock.Object);

            var result = await offerController.AcceptOffer(offerId);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
            transactionMock.Verify(x => x.CreateTransaction(receiverKeys, offer), Times.Once);
            dataManagementMock.Verify(x => x.DeleteData(senderId, receiverId), Times.Once);
        }

        [Fact]
        public async Task AcceptOffer_OfferNotFound()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var offerRepositoryMock = new Mock<IRepository<OfferModel>>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            offerRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<OfferModel>, IQueryable<OfferModel>>>(), CancellationToken.None))
                .ReturnsAsync((OfferModel)null);

            var offerController = new OfferController(null, null, null, null,
                offerRepositoryMock.Object, null, null, null, userInfoMock.Object);

            var result = await offerController.AcceptOffer(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task AcceptOffer_OfferIsAccepted_Conflict()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var offerRepositoryMock = new Mock<IRepository<OfferModel>>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            offerRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<OfferModel>, IQueryable<OfferModel>>>(), CancellationToken.None))
                .ReturnsAsync(new OfferModel { is_accepted = true });

            var offerController = new OfferController(null, null, null, null,
                offerRepositoryMock.Object, null, null, null, userInfoMock.Object);

            var result = await offerController.AcceptOffer(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(409, objectResult.StatusCode);
        }

        [Fact]
        public async Task AcceptOffer_ReceiverKeysIsNull()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var offerRepositoryMock = new Mock<IRepository<OfferModel>>();
            var keyRepositoryMock = new Mock<IRepository<KeyModel>>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            offerRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<OfferModel>, IQueryable<OfferModel>>>(), CancellationToken.None))
                .ReturnsAsync(new OfferModel { is_accepted = false, receiver_id = 1 });
            keyRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<KeyModel>, IQueryable<KeyModel>>>(), CancellationToken.None))
                .ReturnsAsync((KeyModel)null);

            var offerController = new OfferController(null, null, null, null,
                offerRepositoryMock.Object, keyRepositoryMock.Object, null, null, userInfoMock.Object);

            var result = await offerController.AcceptOffer(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task AcceptOffer_TransactionThrowsException()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var offerRepositoryMock = new Mock<IRepository<OfferModel>>();
            var keyRepositoryMock = new Mock<IRepository<KeyModel>>();
            var transactionMock = new Mock<ITransaction<KeyModel>>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            transactionMock.Setup(x => x.CreateTransaction(It.IsAny<KeyModel>(), It.IsAny<OfferModel>()))
                .ThrowsAsync(new EntityNotUpdatedException());
            offerRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<OfferModel>, IQueryable<OfferModel>>>(), CancellationToken.None))
                .ReturnsAsync(new OfferModel { is_accepted = false, receiver_id = 1 });
            keyRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<KeyModel>, IQueryable<KeyModel>>>(), CancellationToken.None))
                .ReturnsAsync(new KeyModel());

            var offerController = new OfferController(null, transactionMock.Object, null, null,
                offerRepositoryMock.Object, keyRepositoryMock.Object, null, null, userInfoMock.Object);

            var result = await offerController.AcceptOffer(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        //[Fact]
        //public async Task GetOneOffer_Success()
        //{
        //    var userInfoMock = new Mock<IUserInfo>();
        //    var cache = new Mock<ICacheHandler<OfferModel>>();

            
        //}
    }
}
