﻿using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using webapi.Controllers.Admin;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Models;

namespace tests.Controllers_Tests.Admin
{
    public class OfferController_Test
    {
        [Fact]
        public async Task GetOffer_Success()
        {
            var offerRepositoryMock = new Mock<IRepository<OfferModel>>();

            offerRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ReturnsAsync(new OfferModel());

            var offerController = new Admin_OfferController(offerRepositoryMock.Object, null, null);
            var result = await offerController.GetOffer(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetOffer_OfferIsNull()
        {
            var offerRepositoryMock = new Mock<IRepository<OfferModel>>();
            offerRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ReturnsAsync((OfferModel)null);

            var offerController = new Admin_OfferController(offerRepositoryMock.Object, null, null);
            var result = await offerController.GetOffer(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetOffer_DbConnectionFailed()
        {
            var offerRepositoryMock = new Mock<IRepository<OfferModel>>();
            offerRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));

            var offerController = new Admin_OfferController(offerRepositoryMock.Object, null, null);
            var result = await offerController.GetOffer(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetRangeOffers_Success()
        {
            var offerRepositoryMock = new Mock<IRepository<OfferModel>>();
            var sortMock = new Mock<ISorting>();

            offerRepositoryMock.Setup(x => x.GetAll(It.IsAny<Func<IQueryable<OfferModel>, IQueryable<OfferModel>>>(), CancellationToken.None))
                .ReturnsAsync(new List<OfferModel>());

            var offerController = new Admin_OfferController(offerRepositoryMock.Object, null, sortMock.Object);
            var result = await offerController.GetRangeOffers(1, 0, 5, true, null, null, null);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetRangeOffers_DbConnectionFailed()
        {
            var offerRepositoryMock = new Mock<IRepository<OfferModel>>();
            var sortMock = new Mock<ISorting>();

            offerRepositoryMock.Setup(x => x.GetAll(It.IsAny<Func<IQueryable<OfferModel>, IQueryable<OfferModel>>>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));

            var offerController = new Admin_OfferController(offerRepositoryMock.Object, null, sortMock.Object);
            var result = await offerController.GetRangeOffers(1, 0, 5, true, null, null, null);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteOffer_Success()
        {
            var offerRepositoryMock = new Mock<IRepository<OfferModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            offerRepositoryMock.Setup(x => x.Delete(It.IsAny<int>(), CancellationToken.None))
                .ReturnsAsync(new OfferModel { sender_id = 1, receiver_id = 2 });

            var offerController = new Admin_OfferController(offerRepositoryMock.Object, redisCacheMock.Object, null);
            var result = await offerController.DeleteOffer(1);

            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task DeleteOffer_EntityNotDeleted()
        {
            var offerRepositoryMock = new Mock<IRepository<OfferModel>>();

            offerRepositoryMock.Setup(x => x.Delete(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(EntityNotDeletedException)));

            var offerController = new Admin_OfferController(offerRepositoryMock.Object, null, null);
            var result = await offerController.DeleteOffer(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteRangeOffers_Success()
        {
            var offerRepositoryMock = new Mock<IRepository<OfferModel>>();
            var redisCacheMock = new Mock<IRedisCache>();

            offerRepositoryMock.Setup(x => x.DeleteMany(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
                .ReturnsAsync(new List<OfferModel> { new OfferModel { sender_id = 1, receiver_id = 2 } });

            var offerController = new Admin_OfferController(offerRepositoryMock.Object, redisCacheMock.Object, null);
            var result = await offerController.DeleteRangeOffers(new List<int> { 1 });

            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task DeleteRangeOffers_EntityNotDeleted()
        {
            var offerRepositoryMock = new Mock<IRepository<OfferModel>>();

            offerRepositoryMock.Setup(x => x.DeleteMany(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(EntityNotDeletedException)));

            var offerController = new Admin_OfferController(offerRepositoryMock.Object, null, null);
            var result = await offerController.DeleteRangeOffers(new List<int> { 1 });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
