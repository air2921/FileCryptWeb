using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Admin;
using webapi.DB.Abstractions;
using webapi.Exceptions;
using webapi.Helpers.Abstractions;
using webapi.Models;
using webapi.Services.Abstractions;

namespace tests.Controllers_Tests.Admin
{
    public class TokenController_Test
    {
        [Fact]
        public async Task RevokeAllUserTokens_Success()
        {
            var id = 1;

            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var tokenRepositoryMock = new Mock<IRepository<TokenModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var transactionMock = new Mock<ITransaction<TokenModel>>();
            var validatorMock = new Mock<IValidator>();

            userRepositoryMock.Setup(x => x.GetById(id, CancellationToken.None)).ReturnsAsync(new UserModel { role = "User", id = id });
            userInfoMock.Setup(x => x.Role).Returns("HighestAdmin");
            validatorMock.Setup(x => x.IsValid("User", "HighestAdmin")).Returns(true);

            var tokenController = new Admin_TokenController(transactionMock.Object, validatorMock.Object,
                tokenRepositoryMock.Object, userRepositoryMock.Object, userInfoMock.Object);
            var result = await tokenController.RevokeAllUserTokens(id);

            transactionMock.Verify(x => x.CreateTransaction(null, id), Times.Once);
            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task RevokeAllUserTokens_UserNotFound()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);

            var tokenController = new Admin_TokenController(null, null, null, userRepositoryMock.Object, null);
            var result = await tokenController.RevokeAllUserTokens(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task RevokeAllUserTokens_DbConnectionFailed()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            var tokenController = new Admin_TokenController(null, null, null, userRepositoryMock.Object, null);
            var result = await tokenController.RevokeAllUserTokens(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task RevokeAllUserTokens_Forbidden()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var validatorMock = new Mock<IValidator>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());
            userInfoMock.Setup(x => x.Role).Returns(string.Empty);
            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var tokenController = new Admin_TokenController(null, validatorMock.Object,
                null, userRepositoryMock.Object, userInfoMock.Object);
            var result = await tokenController.RevokeAllUserTokens(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(403, objectResult.StatusCode);
        }
        
        [Theory]
        [InlineData(typeof(EntityNotCreatedException))]
        [InlineData(typeof(EntityNotDeletedException))]
        [InlineData(typeof(OperationCanceledException))]
        public async Task RevokeAllUserTokens_DbTransactionFailed(Type ex)
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var tokenRepositoryMock = new Mock<IRepository<TokenModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var transactionMock = new Mock<ITransaction<TokenModel>>();
            var validatorMock = new Mock<IValidator>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel { role = "User"});
            userInfoMock.Setup(x => x.Role).Returns("HighestAdmin");
            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            transactionMock.Setup(x => x.CreateTransaction(null, It.IsAny<int>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(ex));

            var tokenController = new Admin_TokenController(transactionMock.Object, validatorMock.Object, tokenRepositoryMock.Object,
                userRepositoryMock.Object, userInfoMock.Object);
            var result = await tokenController.RevokeAllUserTokens(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task RevokeToken_Success()
        {
            var id = 1;
            var userId = 3;

            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var tokenRepositoryMock = new Mock<IRepository<TokenModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var validatorMock = new Mock<IValidator>();

            userInfoMock.Setup(x => x.Role).Returns("HighestAdmin");
            tokenRepositoryMock.Setup(x => x.GetById(id, CancellationToken.None)).ReturnsAsync(new TokenModel { user_id = userId });
            userRepositoryMock.Setup(x => x.GetById(userId, CancellationToken.None)).ReturnsAsync(new UserModel { role  = "User" });
            validatorMock.Setup(x => x.IsValid("User", "HighestAdmin")).Returns(true);

            var tokenController = new Admin_TokenController(null, validatorMock.Object, tokenRepositoryMock.Object,
                userRepositoryMock.Object, userInfoMock.Object);
            var result = await tokenController.RevokeToken(id);

            tokenRepositoryMock.Verify(x => x.Delete(id, CancellationToken.None), Times.Once);
            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task RevokeToken_TokenNotFound()
        {
            var tokenRepositoryMock = new Mock<IRepository<TokenModel>>();
            tokenRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ReturnsAsync((TokenModel)null);

            var tokenController = new Admin_TokenController(null, null, tokenRepositoryMock.Object,
                null, null);
            var result = await tokenController.RevokeToken(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task RevokeToken_UserNotFound()
        {
            var tokenRepositoryMock = new Mock<IRepository<TokenModel>>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            tokenRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ReturnsAsync(new TokenModel { user_id = 1 });
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ReturnsAsync((UserModel)null);

            var tokenController = new Admin_TokenController(null, null, tokenRepositoryMock.Object, userRepositoryMock.Object, null);
            var result = await tokenController.RevokeToken(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task RevokeToken_DbConnectionFailed()
        {
            var tokenRepositoryMock = new Mock<IRepository<TokenModel>>();
            tokenRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            var tokenController = new Admin_TokenController(null, null, tokenRepositoryMock.Object, null, null);
            var result = await tokenController.RevokeToken(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task RevokeToken_Forbidden()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var tokenRepositoryMock = new Mock<IRepository<TokenModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var validatorMock = new Mock<IValidator>();

            tokenRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new TokenModel { user_id = 1 });
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel { role = string.Empty });
            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var tokenController = new Admin_TokenController(null, validatorMock.Object, tokenRepositoryMock.Object,
                userRepositoryMock.Object, userInfoMock.Object);
            var result = await tokenController.RevokeToken(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(403, objectResult.StatusCode);
        }

        [Fact]
        public async Task RevokeToken_EntityNotDeleted()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var tokenRepositoryMock = new Mock<IRepository<TokenModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var validatorMock = new Mock<IValidator>();

            tokenRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new TokenModel { user_id = 1 });
            tokenRepositoryMock.Setup(x => x.Delete(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new EntityNotDeletedException());
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel { role = string.Empty });
            validatorMock.Setup(x => x.IsValid(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            var tokenController = new Admin_TokenController(null, validatorMock.Object, tokenRepositoryMock.Object,
                userRepositoryMock.Object, userInfoMock.Object);
            var result = await tokenController.RevokeToken(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
