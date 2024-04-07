using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Account;
using webapi.DB.Abstractions;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Third_Party_Services.Abstractions;
using webapi.Models;
using webapi.Services.Abstractions;
using webapi.Services.Account;
using webapi.DB.Ef.Specifications;

namespace tests.Controllers_Tests.Account
{
    public class RecoveryController_Test
    {
        [Fact]
        public async Task RecoveryAccount_Success()
        {
            var id = 1;
            var email = "air23663@gmail.com";
            var username = "air2921";
            var user = new UserModel
            {
                id = id,
                username = username,
                email = email
            };

            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var generateMock = new Mock<IGenerate>();
            var emailSenderMock = new Mock<IEmailSender>();
            var redisCacheMock = new Mock<IRedisCache>();
            var fileManagerMock = new Mock<IFileManager>();
            var recoveryServiceMock = new Mock<IRecoveryHelpers>();

            userRepositoryMock.Setup(x => x.GetByFilter(new UserByEmailSpec(email.ToLowerInvariant()), CancellationToken.None))
                .ReturnsAsync(user);
            generateMock.Setup(x => x.GenerateKey()).Returns("8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8=");
            fileManagerMock.Setup(x => x.GetReactAppUrl()).Returns(string.Empty);

            var recoveryController = new RecoveryController(recoveryServiceMock.Object, null, userRepositoryMock.Object,
                null, emailSenderMock.Object, redisCacheMock.Object, fileManagerMock.Object, generateMock.Object);

            var result = await recoveryController.RecoveryAccount(email);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(201, objectResult.StatusCode);

            recoveryServiceMock.Verify(x => x.CreateTokenTransaction(user, It.Is<string>(q => q.Length >= 112)), Times.Once);
            emailSenderMock.Verify(x => x.SendMessage(It.Is<EmailDto>(e => e.username == username && e.email == email)), Times.Once);
            redisCacheMock.Verify(x => x.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{id}"), Times.Once);
        }

        [Fact]
        public async Task RecoveryAccount_UserNotFound()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<UserByEmailSpec>(), CancellationToken.None))
                .ReturnsAsync((UserModel)null);

            var recoveryController = new RecoveryController(null, null, userRepositoryMock.Object, null, null, null, null, null);

            var result = await recoveryController.RecoveryAccount(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task RecoveryAccount_DbConnectionFailed()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<UserByEmailSpec>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            var recoveryController = new RecoveryController(null, null, userRepositoryMock.Object, null, null, null, null, null);

            var result = await recoveryController.RecoveryAccount(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task RecoveryAccount_CreateLinkFailed_ThrowsException()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var generateMock = new Mock<IGenerate>();
            var recoveryServiceMock = new Mock<IRecoveryHelpers>();

            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<UserByEmailSpec>(), CancellationToken.None))
                .ReturnsAsync(new UserModel
                {
                    username = string.Empty,
                    email = string.Empty
                });
            generateMock.Setup(x => x.GenerateKey()).Returns(string.Empty);
            recoveryServiceMock.Setup(x => x.CreateTokenTransaction(It.IsAny<UserModel>(), It.IsAny<string>()))
                .ThrowsAsync(new EntityNotCreatedException());

            var recoveryController = new RecoveryController(recoveryServiceMock.Object, null,
                userRepositoryMock.Object, null, null, null, null, generateMock.Object);

            var result = await recoveryController.RecoveryAccount(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task RecoveryAccount_EmailSendFailed()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var generateMock = new Mock<IGenerate>();
            var emailSenderMock = new Mock<IEmailSender>();
            var fileManagerMock = new Mock<IFileManager>();
            var recoveryServiceMock = new Mock<IRecoveryHelpers>();

            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<UserByEmailSpec>(), CancellationToken.None))
                .ReturnsAsync(new UserModel
                {
                    username = string.Empty,
                    email = string.Empty
                });
            generateMock.Setup(x => x.GenerateKey()).Returns(string.Empty);
            fileManagerMock.Setup(x => x.GetReactAppUrl()).Returns(string.Empty);
            emailSenderMock.Setup(x => x.SendMessage(It.IsAny<EmailDto>()))
                .ThrowsAsync(new SmtpClientException());

            var recoveryController = new RecoveryController(recoveryServiceMock.Object, null,
                userRepositoryMock.Object, null, emailSenderMock.Object, null, fileManagerMock.Object, generateMock.Object);

            var result = await recoveryController.RecoveryAccount(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task RecoveryAccountByToken_Success()
        {
            var userId = 1;
            var linkId = 1;
            var password = "password";
            var token = "fdjklgjhfdjkghdfjkghbsdfkgjhildfug9opsdfuig90dsf";
            var user = new UserModel { id = userId };
            var link = new LinkModel
            {
                link_id = linkId,
                user_id = userId,
                expiry_date = DateTime.UtcNow.AddDays(1)
            };

            var validatorMock = new Mock<IValidator>();
            var linkRepositoryMock = new Mock<IRepository<LinkModel>>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var redisCacheMock = new Mock<IRedisCache>();
            var recoveryServiceMock = new Mock<IRecoveryHelpers>();

            validatorMock.Setup(x => x.IsValid(password, null)).Returns(true);
            linkRepositoryMock.Setup(x => x.GetByFilter(new RecoveryTokenByTokenSpec(token), CancellationToken.None))
                .ReturnsAsync(link); ;
            userRepositoryMock.Setup(x => x.GetById(userId, CancellationToken.None)).ReturnsAsync(user);

            var recoveryController = new RecoveryController(recoveryServiceMock.Object, validatorMock.Object, userRepositoryMock.Object,
                linkRepositoryMock.Object, null, redisCacheMock.Object, null, null);

            var result = await recoveryController.RecoveryAccountByToken(new RecoveryDTO { password = password, token = token });

            Assert.Equal(200, ((StatusCodeResult)result).StatusCode);

            recoveryServiceMock.Verify(x => x.RecoveryTransaction(user, token, password), Times.Once);
            redisCacheMock.Verify(x => x.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{userId}"), Times.Once);
            redisCacheMock.Verify(x => x.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{userId}"), Times.Once);
        }

        [Fact]
        public async Task RecoveryAccountByToken_InvalidPassword()
        {
            var validatorMock = new Mock<IValidator>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(false);

            var recoveryController = new RecoveryController(null, validatorMock.Object, null, null, null, null, null, null);

            var result = await recoveryController.RecoveryAccountByToken(new RecoveryDTO { password = string.Empty, token = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task RecoveryAccountByToken_LinkNotFound()
        {
            var validatorMock = new Mock<IValidator>();
            var linkRepositoryMock = new Mock<IRepository<LinkModel>>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);
            linkRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<RecoveryTokenByTokenSpec>(), CancellationToken.None))
                .ReturnsAsync((LinkModel)null);

            var recoveryController = new RecoveryController(null, validatorMock.Object, null, linkRepositoryMock.Object,
                null, null, null, null);

            var result = await recoveryController.RecoveryAccountByToken(new RecoveryDTO { password = string.Empty, token = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task RecoveryAccountByToken_LinkExpired()
        {
            var validatorMock = new Mock<IValidator>();
            var linkRepositoryMock = new Mock<IRepository<LinkModel>>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);
            linkRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<RecoveryTokenByTokenSpec>(), CancellationToken.None))
                .ReturnsAsync(new LinkModel
                {
                    link_id = 1,
                    user_id = 1,
                    expiry_date = DateTime.UtcNow.AddDays(-1)
                });

            var recoveryController = new RecoveryController(null, validatorMock.Object, null, linkRepositoryMock.Object,
                null, null, null, null);

            var result = await recoveryController.RecoveryAccountByToken(new RecoveryDTO { password = string.Empty, token = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(422, objectResult.StatusCode);
        }

        [Fact]
        public async Task RecoveryAccountByToken_UserNotFound()
        {
            var validatorMock = new Mock<IValidator>();
            var linkRepositoryMock = new Mock<IRepository<LinkModel>>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);
            linkRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<RecoveryTokenByTokenSpec>(), CancellationToken.None))
                .ReturnsAsync(new LinkModel
                {
                    link_id = 1,
                    user_id = 1,
                    expiry_date = DateTime.UtcNow.AddDays(1)
                });
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);

            var recoveryController = new RecoveryController(null, validatorMock.Object, userRepositoryMock.Object, linkRepositoryMock.Object,
                null, null, null, null);

            var result = await recoveryController.RecoveryAccountByToken(new RecoveryDTO { password = string.Empty, token = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task RecoveryAccountByToken_DbConnectionFailed()
        {
            var validatorMock = new Mock<IValidator>();
            var linkRepositoryMock = new Mock<IRepository<LinkModel>>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);
            linkRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<RecoveryTokenByTokenSpec>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            var recoveryController = new RecoveryController(null, validatorMock.Object, null, linkRepositoryMock.Object,
                null, null, null, null);

            var result = await recoveryController.RecoveryAccountByToken(new RecoveryDTO { password = string.Empty, token = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task RecoveryAccountByToken_LinkNotDeleted()
        {
            var validatorMock = new Mock<IValidator>();
            var linkRepositoryMock = new Mock<IRepository<LinkModel>>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);
            linkRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<RecoveryTokenByTokenSpec>(), CancellationToken.None))
                .ReturnsAsync(new LinkModel
                {
                    link_id = 1,
                    user_id = 1,
                    expiry_date = DateTime.UtcNow.AddDays(-1)
                });
            linkRepositoryMock.Setup(x => x.Delete(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new EntityNotDeletedException());

            var recoveryController = new RecoveryController(null, validatorMock.Object, null, linkRepositoryMock.Object,
                null, null, null, null);

            var result = await recoveryController.RecoveryAccountByToken(new RecoveryDTO { password = string.Empty, token = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Theory]
        [InlineData(typeof(EntityNotDeletedException))]
        [InlineData(typeof(EntityNotUpdatedException))]
        [InlineData(typeof(EntityNotCreatedException))]
        public async Task RecoveryAccountByToken_RecoveryAccountTransactionFailed(Type ex)
        {
            var recoveryServiceMock = new Mock<IRecoveryHelpers>();
            var validatorMock = new Mock<IValidator>();
            var linkRepositoryMock = new Mock<IRepository<LinkModel>>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), null)).Returns(true);
            recoveryServiceMock.Setup(x => x.RecoveryTransaction(It.IsAny<UserModel>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(ex));
            linkRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<RecoveryTokenByTokenSpec>(), CancellationToken.None))
                .ReturnsAsync(new LinkModel
                {
                    link_id = 1,
                    user_id = 1,
                    expiry_date = DateTime.UtcNow.AddDays(1)
                });
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var recoveryController = new RecoveryController(recoveryServiceMock.Object, validatorMock.Object, userRepositoryMock.Object,
                linkRepositoryMock.Object, null, null, null, null);

            var result = await recoveryController.RecoveryAccountByToken(new RecoveryDTO { password = string.Empty, token = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
