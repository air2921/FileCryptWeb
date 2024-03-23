using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Account;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Models;

namespace tests.Controllers_Tests.Account
{
    //public class RecoveryController_Test
    //{
    //    [Fact]
    //    public async Task RecoveryAccount_Success()
    //    {
    //        var userRepositoryMock = new Mock<IRepository<UserModel>>();
    //        var generateMock = new Mock<IGenerate>();
    //        var recoveryServiceMock = new Mock<IRecoveryService>();
    //        var redisCacheMock = new Mock<IRedisCache>();

    //        userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
    //            .ReturnsAsync(new UserModel
    //            {
    //                username = string.Empty,
    //                email = string.Empty
    //            });
    //        generateMock.Setup(x => x.GenerateKey()).Returns(string.Empty);

    //        var recoveryController = new RecoveryController(recoveryServiceMock.Object, userRepositoryMock.Object,
    //            redisCacheMock.Object, null, null, generateMock.Object);

    //        var result = await recoveryController.RecoveryAccount(string.Empty);

    //        Assert.IsType<ObjectResult>(result);
    //        var objectResult = (ObjectResult)result;
    //        Assert.Equal(201, objectResult.StatusCode);
    //    }

    //    [Fact]
    //    public async Task RecoveryAccount_UserNotFound()
    //    {
    //        var userRepositoryMock = new Mock<IRepository<UserModel>>();

    //        userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
    //            .ReturnsAsync((UserModel)null);

    //        var recoveryController = new RecoveryController(null, userRepositoryMock.Object,
    //            null, null, null, null);

    //        var result = await recoveryController.RecoveryAccount(string.Empty);

    //        Assert.IsType<ObjectResult>(result);
    //        var objectResult = (ObjectResult)result;
    //        Assert.Equal(404, objectResult.StatusCode);
    //    }

    //    [Fact]
    //    public async Task RecoveryAccount_DbConnectionFailed()
    //    {
    //        var userRepositoryMock = new Mock<IRepository<UserModel>>();

    //        userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
    //            .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));

    //        var recoveryController = new RecoveryController(null, userRepositoryMock.Object,
    //            null, null, null, null);

    //        var result = await recoveryController.RecoveryAccount(string.Empty);

    //        Assert.IsType<ObjectResult>(result);
    //        var objectResult = (ObjectResult)result;
    //        Assert.Equal(500, objectResult.StatusCode);
    //    }

    //    [Fact]
    //    public async Task RecoveryAccount_CreateLinkFailed_ThrowsException()
    //    {
    //        var userRepositoryMock = new Mock<IRepository<UserModel>>();
    //        var generateMock = new Mock<IGenerate>();
    //        var recoveryServiceMock = new Mock<IRecoveryService>();

    //        userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
    //            .ReturnsAsync(new UserModel
    //            {
    //                username = string.Empty,
    //                email = string.Empty
    //            });
    //        generateMock.Setup(x => x.GenerateKey()).Returns(string.Empty);
    //        recoveryServiceMock.Setup(x => x.CreateTokenTransaction(It.IsAny<UserModel>(), It.IsAny<string>()))
    //            .ThrowsAsync((Exception)Activator.CreateInstance(typeof(EntityNotCreatedException)));

    //        var recoveryController = new RecoveryController(recoveryServiceMock.Object, userRepositoryMock.Object,
    //            null, null, null, generateMock.Object);

    //        var result = await recoveryController.RecoveryAccount(string.Empty);

    //        Assert.IsType<ObjectResult>(result);
    //        var objectResult = (ObjectResult)result;
    //        Assert.Equal(500, objectResult.StatusCode);
    //    }

    //    [Fact]
    //    public async Task RecoveryAccount_EmailSendFailed()
    //    {
    //        var userRepositoryMock = new Mock<IRepository<UserModel>>();
    //        var generateMock = new Mock<IGenerate>();
    //        var recoveryServiceMock = new Mock<IRecoveryService>();

    //        userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
    //            .ReturnsAsync(new UserModel
    //            {
    //                username = string.Empty,
    //                email = string.Empty
    //            });
    //        generateMock.Setup(x => x.GenerateKey()).Returns(string.Empty);
    //        recoveryServiceMock.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
    //            .ThrowsAsync((Exception)Activator.CreateInstance(typeof(SmtpClientException)));

    //        var recoveryController = new RecoveryController(recoveryServiceMock.Object, userRepositoryMock.Object,
    //            null, null, null, generateMock.Object);

    //        var result = await recoveryController.RecoveryAccount(string.Empty);

    //        Assert.IsType<ObjectResult>(result);
    //        var objectResult = (ObjectResult)result;
    //        Assert.Equal(500, objectResult.StatusCode);
    //    }

    //    [Fact]
    //    public async Task RecoveryAccountByToken_Success()
    //    {
    //        var recoveryServiceMock = new Mock<IRecoveryService>();
    //        var linkRepositoryMock = new Mock<IRepository<LinkModel>>();
    //        var userRepositoryMock = new Mock<IRepository<UserModel>>();
    //        var redisCacheMock = new Mock<IRedisCache>();

    //        recoveryServiceMock.Setup(x => x.IsValidPassword(It.IsAny<string>())).Returns(true);
    //        linkRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<LinkModel>, IQueryable<LinkModel>>>(), CancellationToken.None))
    //            .ReturnsAsync(new LinkModel
    //            {
    //                link_id = 1,
    //                user_id = 1,
    //                expiry_date = DateTime.UtcNow.AddDays(1)
    //            });
    //        userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

    //        var recoveryController = new RecoveryController(recoveryServiceMock.Object, userRepositoryMock.Object,
    //            redisCacheMock.Object, linkRepositoryMock.Object, null, null);

    //        var result = await recoveryController.RecoveryAccountByToken(new RecoveryDTO { password = string.Empty, token = string.Empty });

    //        Assert.Equal(200, ((StatusCodeResult)result).StatusCode);
    //    }

    //    [Fact]
    //    public async Task RecoveryAccountByToken_InvalidPassword()
    //    {
    //        var recoveryServiceMock = new Mock<IRecoveryService>();

    //        recoveryServiceMock.Setup(x => x.IsValidPassword(It.IsAny<string>())).Returns(false);

    //        var recoveryController = new RecoveryController(recoveryServiceMock.Object, null,
    //            null, null, null, null);

    //        var result = await recoveryController.RecoveryAccountByToken(new RecoveryDTO { password = string.Empty, token = string.Empty });

    //        Assert.IsType<ObjectResult>(result);
    //        var objectResult = (ObjectResult)result;
    //        Assert.Equal(400, objectResult.StatusCode);
    //    }

    //    [Fact]
    //    public async Task RecoveryAccountByToken_LinkNotFound()
    //    {
    //        var recoveryServiceMock = new Mock<IRecoveryService>();
    //        var linkRepositoryMock = new Mock<IRepository<LinkModel>>();

    //        recoveryServiceMock.Setup(x => x.IsValidPassword(It.IsAny<string>())).Returns(true);
    //        linkRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<LinkModel>, IQueryable<LinkModel>>>(), CancellationToken.None))
    //            .ReturnsAsync((LinkModel)null);

    //        var recoveryController = new RecoveryController(recoveryServiceMock.Object, null,
    //            null, linkRepositoryMock.Object, null, null);

    //        var result = await recoveryController.RecoveryAccountByToken(new RecoveryDTO { password = string.Empty, token = string.Empty });

    //        Assert.IsType<ObjectResult>(result);
    //        var objectResult = (ObjectResult)result;
    //        Assert.Equal(404, objectResult.StatusCode);
    //    }

    //    [Fact]
    //    public async Task RecoveryAccountByToken_LinkExpired()
    //    {
    //        var recoveryServiceMock = new Mock<IRecoveryService>();
    //        var linkRepositoryMock = new Mock<IRepository<LinkModel>>();

    //        recoveryServiceMock.Setup(x => x.IsValidPassword(It.IsAny<string>())).Returns(true);
    //        linkRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<LinkModel>, IQueryable<LinkModel>>>(), CancellationToken.None))
    //            .ReturnsAsync(new LinkModel
    //            {
    //                link_id = 1,
    //                user_id = 1,
    //                expiry_date = DateTime.UtcNow.AddDays(-1)
    //            });

    //        var recoveryController = new RecoveryController(recoveryServiceMock.Object, null,
    //            null, linkRepositoryMock.Object, null, null);

    //        var result = await recoveryController.RecoveryAccountByToken(new RecoveryDTO { password = string.Empty, token = string.Empty });

    //        Assert.IsType<ObjectResult>(result);
    //        var objectResult = (ObjectResult)result;
    //        Assert.Equal(422, objectResult.StatusCode);
    //    }

    //    [Fact]
    //    public async Task RecoveryAccountByToken_UserNotFound()
    //    {
    //        var recoveryServiceMock = new Mock<IRecoveryService>();
    //        var linkRepositoryMock = new Mock<IRepository<LinkModel>>();
    //        var userRepositoryMock = new Mock<IRepository<UserModel>>();

    //        recoveryServiceMock.Setup(x => x.IsValidPassword(It.IsAny<string>())).Returns(true);
    //        linkRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<LinkModel>, IQueryable<LinkModel>>>(), CancellationToken.None))
    //            .ReturnsAsync(new LinkModel
    //            {
    //                link_id = 1,
    //                user_id = 1,
    //                expiry_date = DateTime.UtcNow.AddDays(1)
    //            });
    //        userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);

    //        var recoveryController = new RecoveryController(recoveryServiceMock.Object, userRepositoryMock.Object,
    //            null, linkRepositoryMock.Object, null, null);

    //        var result = await recoveryController.RecoveryAccountByToken(new RecoveryDTO { password = string.Empty, token = string.Empty });

    //        Assert.IsType<ObjectResult>(result);
    //        var objectResult = (ObjectResult)result;
    //        Assert.Equal(404, objectResult.StatusCode);
    //    }

    //    [Fact]
    //    public async Task RecoveryAccountByToken_DbConnectionFailed()
    //    {
    //        var recoveryServiceMock = new Mock<IRecoveryService>();
    //        var linkRepositoryMock = new Mock<IRepository<LinkModel>>();

    //        recoveryServiceMock.Setup(x => x.IsValidPassword(It.IsAny<string>())).Returns(true);
    //        linkRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<LinkModel>, IQueryable<LinkModel>>>(), CancellationToken.None))
    //            .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));

    //        var recoveryController = new RecoveryController(recoveryServiceMock.Object, null,
    //            null, linkRepositoryMock.Object, null, null);

    //        var result = await recoveryController.RecoveryAccountByToken(new RecoveryDTO { password = string.Empty, token = string.Empty });

    //        Assert.IsType<ObjectResult>(result);
    //        var objectResult = (ObjectResult)result;
    //        Assert.Equal(500, objectResult.StatusCode);
    //    }

    //    [Fact]
    //    public async Task RecoveryAccountByToken_LinkNotDeleted()
    //    {
    //        var recoveryServiceMock = new Mock<IRecoveryService>();
    //        var linkRepositoryMock = new Mock<IRepository<LinkModel>>();

    //        recoveryServiceMock.Setup(x => x.IsValidPassword(It.IsAny<string>())).Returns(true);
    //        linkRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<LinkModel>, IQueryable<LinkModel>>>(), CancellationToken.None))
    //            .ReturnsAsync(new LinkModel
    //            {
    //                link_id = 1,
    //                user_id = 1,
    //                expiry_date = DateTime.UtcNow.AddDays(-1)
    //            });
    //        linkRepositoryMock.Setup(x => x.Delete(It.IsAny<int>(), CancellationToken.None))
    //            .ThrowsAsync((Exception)Activator.CreateInstance(typeof(EntityNotDeletedException)));

    //        var recoveryController = new RecoveryController(recoveryServiceMock.Object, null,
    //            null, linkRepositoryMock.Object, null, null);

    //        var result = await recoveryController.RecoveryAccountByToken(new RecoveryDTO { password = string.Empty, token = string.Empty });

    //        Assert.IsType<ObjectResult>(result);
    //        var objectResult = (ObjectResult)result;
    //        Assert.Equal(500, objectResult.StatusCode);
    //    }

    //    [Theory]
    //    [InlineData(typeof(EntityNotDeletedException))]
    //    [InlineData(typeof(EntityNotUpdatedException))]
    //    [InlineData(typeof(EntityNotCreatedException))]
    //    public async Task RecoveryAccountByToken_RecoveryAccountTransactionFailed(Type ex)
    //    {
    //        var recoveryServiceMock = new Mock<IRecoveryService>();
    //        var linkRepositoryMock = new Mock<IRepository<LinkModel>>();
    //        var userRepositoryMock = new Mock<IRepository<UserModel>>();

    //        recoveryServiceMock.Setup(x => x.IsValidPassword(It.IsAny<string>())).Returns(true);
    //        recoveryServiceMock.Setup(x => x.RecoveryTransaction(It.IsAny<UserModel>(), It.IsAny<string>(), It.IsAny<string>()))
    //            .ThrowsAsync((Exception)Activator.CreateInstance(ex));
    //        linkRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<LinkModel>, IQueryable<LinkModel>>>(), CancellationToken.None))
    //            .ReturnsAsync(new LinkModel
    //            {
    //                link_id = 1,
    //                user_id = 1,
    //                expiry_date = DateTime.UtcNow.AddDays(1)
    //            });
    //        userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

    //        var recoveryController = new RecoveryController(recoveryServiceMock.Object, userRepositoryMock.Object,
    //            null, linkRepositoryMock.Object, null, null);

    //        var result = await recoveryController.RecoveryAccountByToken(new RecoveryDTO { password = string.Empty, token = string.Empty });

    //        Assert.IsType<ObjectResult>(result);
    //        var objectResult = (ObjectResult)result;
    //        Assert.Equal(500, objectResult.StatusCode);
    //    }
    //}
}
