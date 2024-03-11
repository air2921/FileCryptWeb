using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Admin;
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

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());
            tokenRepositoryMock.Setup(x => x.GetAll(It.IsAny<Func<IQueryable<TokenModel>, IQueryable<TokenModel>>>(), CancellationToken.None))
                .ReturnsAsync(new List<TokenModel>());
            userInfoMock.Setup(x => x.Role).Returns(string.Empty);
            apiTokenServiceMock.Setup(x => x.IsAllowed(It.IsAny<UserModel>(), It.IsAny<string>())).Returns(true);

            var tokenController = new Admin_TokenController(apiTokenServiceMock.Object, tokenRepositoryMock.Object,
                userRepositoryMock.Object, userInfoMock.Object);
            var result = await tokenController.RevokeAllUserTokens(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }
    }
}
