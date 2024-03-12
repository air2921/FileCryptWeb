using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Admin;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Services;
using webapi.Models;

namespace tests.Controllers_Tests.Admin
{
    public class TokenController_Test
    {
        [Fact]
        public async Task RevokeAllUserTokens_Success()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var tokenRepositoryMock = new Mock<IRepository<TokenModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var apiTokenServiceMock = new Mock<IApiAdminTokenService>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel { role = "User"});
            tokenRepositoryMock.Setup(x => x.GetAll(It.IsAny<Func<IQueryable<TokenModel>, IQueryable<TokenModel>>>(), CancellationToken.None))
                .ReturnsAsync(new List<TokenModel>());
            userInfoMock.Setup(x => x.Role).Returns("HighestAdmin");
            apiTokenServiceMock.Setup(x => x.IsAllowed(It.IsAny<UserModel>(), It.IsAny<string>())).Returns(true);

            var tokenController = new Admin_TokenController(apiTokenServiceMock.Object, tokenRepositoryMock.Object,
                userRepositoryMock.Object, userInfoMock.Object);
            var result = await tokenController.RevokeAllUserTokens(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task RevokeAllUserTokens_UserNotFound()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);

            var tokenController = new Admin_TokenController(null, null, userRepositoryMock.Object, null);
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
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));

            var tokenController = new Admin_TokenController(null, null, userRepositoryMock.Object, null);
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
            var apiTokenServiceMock = new Mock<IApiAdminTokenService>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());
            userInfoMock.Setup(x => x.Role).Returns(string.Empty);
            apiTokenServiceMock.Setup(x => x.IsAllowed(It.IsAny<UserModel>(), It.IsAny<string>())).Returns(false);

            var tokenController = new Admin_TokenController(apiTokenServiceMock.Object, null, userRepositoryMock.Object, userInfoMock.Object);
            var result = await tokenController.RevokeAllUserTokens(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(403, objectResult.StatusCode);
        }
        
        [Theory]
        [InlineData(typeof(EntityNotCreatedException))]
        [InlineData(typeof(EntityNotDeletedException))]
        public async Task RevokeAllUserTokens_DbTransactionFailed(Type ex)
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var tokenRepositoryMock = new Mock<IRepository<TokenModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var apiTokenServiceMock = new Mock<IApiAdminTokenService>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel { role = "User"});
            tokenRepositoryMock.Setup(x => x.GetAll(It.IsAny<Func<IQueryable<TokenModel>, IQueryable<TokenModel>>>(), CancellationToken.None))
                .ReturnsAsync(new List<TokenModel>());
            userInfoMock.Setup(x => x.Role).Returns("HighestAdmin");
            apiTokenServiceMock.Setup(x => x.IsAllowed(It.IsAny<UserModel>(), It.IsAny<string>())).Returns(true);
            apiTokenServiceMock.Setup(x => x.DbTransaction(It.IsAny<IEnumerable<TokenModel>>(), It.IsAny<int>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(ex));

            var tokenController = new Admin_TokenController(apiTokenServiceMock.Object, tokenRepositoryMock.Object,
                userRepositoryMock.Object, userInfoMock.Object);
            var result = await tokenController.RevokeAllUserTokens(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task RevokeToken_Success()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var tokenRepositoryMock = new Mock<IRepository<TokenModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var apiTokenServiceMock = new Mock<IApiAdminTokenService>();

            tokenRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new TokenModel { user_id = 1 });
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel { role  = string.Empty });
            apiTokenServiceMock.Setup(x => x.IsAllowed(It.IsAny<UserModel>(), It.IsAny<string>())).Returns(true);

            var tokenController = new Admin_TokenController(apiTokenServiceMock.Object, tokenRepositoryMock.Object,
                userRepositoryMock.Object, userInfoMock.Object);
            var result = await tokenController.RevokeToken(1);

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

            var tokenController = new Admin_TokenController(null, tokenRepositoryMock.Object, null, null);
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

            var tokenController = new Admin_TokenController(null, tokenRepositoryMock.Object, userRepositoryMock.Object, null);
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
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));

            var tokenController = new Admin_TokenController(null, tokenRepositoryMock.Object, null, null);
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
            var apiTokenServiceMock = new Mock<IApiAdminTokenService>();

            tokenRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new TokenModel { user_id = 1 });
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel { role = string.Empty });
            apiTokenServiceMock.Setup(x => x.IsAllowed(It.IsAny<UserModel>(), It.IsAny<string>())).Returns(false);

            var tokenController = new Admin_TokenController(apiTokenServiceMock.Object, tokenRepositoryMock.Object,
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
            var apiTokenServiceMock = new Mock<IApiAdminTokenService>();

            tokenRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new TokenModel { user_id = 1 });
            tokenRepositoryMock.Setup(x => x.Delete(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(EntityNotDeletedException)));
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel { role = string.Empty });
            apiTokenServiceMock.Setup(x => x.IsAllowed(It.IsAny<UserModel>(), It.IsAny<string>())).Returns(true);

            var tokenController = new Admin_TokenController(apiTokenServiceMock.Object, tokenRepositoryMock.Object,
                userRepositoryMock.Object, userInfoMock.Object);
            var result = await tokenController.RevokeToken(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
