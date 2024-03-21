using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Account;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Services;
using webapi.Models;

namespace tests.Controllers_Tests.Account
{
    public class SessionController_Test
    {
        [Fact]
        public async Task Login_2FaDisable_Success()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var generateMock = new Mock<IGenerate>();
            var sessionServiceMock = new Mock<ISessionService>();

            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ReturnsAsync(new UserModel
                {
                    id = 1,
                    username = string.Empty,
                    email = string.Empty,
                    password = string.Empty,
                    is_blocked = false,
                    is_2fa_enabled = false
                });
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(123);
            sessionServiceMock.Setup(x => x.CreateTokens(It.IsAny<UserModel>(), It.IsAny<HttpContext>()))
                .Returns(Task.FromResult<IActionResult>(new StatusCodeResult(200)));

            var sessionController = new AuthSessionController(sessionServiceMock.Object, userRepositoryMock.Object,
                passwordManagerMock.Object, null, generateMock.Object);

            var result = await sessionController.Login(new AuthDTO { email = string.Empty, password = string.Empty });

            Assert.Equal(200, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task Login_2FaEnable_Success()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var generateMock = new Mock<IGenerate>();
            var sessionServiceMock = new Mock<ISessionService>();

            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ReturnsAsync(new UserModel
                {
                    id = 1,
                    username = string.Empty,
                    email = string.Empty,
                    password = string.Empty,
                    is_blocked = false,
                    is_2fa_enabled = true
                });
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(123);
            sessionServiceMock.Setup(x => x.CreateTokens(It.IsAny<UserModel>(), It.IsAny<HttpContext>()))
                .Returns(Task.FromResult<IActionResult>(new ObjectResult(new { message = string.Empty, confirm = true }) { StatusCode = 200 }));

            var sessionController = new AuthSessionController(sessionServiceMock.Object, userRepositoryMock.Object,
                passwordManagerMock.Object, null, generateMock.Object);

            var result = await sessionController.Login(new AuthDTO { email = string.Empty, password = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task Login_UserNotFound()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ReturnsAsync((UserModel)null);

            var sessionController = new AuthSessionController(null, userRepositoryMock.Object, null, null, null);

            var result = await sessionController.Login(new AuthDTO { email = string.Empty, password = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task Login_DbConnectionFailed()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));

            var sessionController = new AuthSessionController(null, userRepositoryMock.Object, null, null, null);

            var result = await sessionController.Login(new AuthDTO { email = string.Empty, password = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task Login_UserBlocked()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ReturnsAsync(new UserModel
                {
                    id = 1,
                    is_blocked = true
                });

            var sessionController = new AuthSessionController(null, userRepositoryMock.Object, null, null, null);

            var result = await sessionController.Login(new AuthDTO { email = string.Empty, password = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(403, objectResult.StatusCode);
        }

        [Fact]
        public async Task Login_PasswordIncorrect()
        {
            var passwordManagerMock = new Mock<IPasswordManager>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ReturnsAsync(new UserModel
                {
                    id = 1,
                    is_blocked = false
                });

            var sessionController = new AuthSessionController(null, userRepositoryMock.Object, passwordManagerMock.Object, null, null);

            var result = await sessionController.Login(new AuthDTO { email = string.Empty, password = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(401, objectResult.StatusCode);
        }

        [Fact]
        public async Task Login_SendEmailFailed()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var generateMock = new Mock<IGenerate>();
            var sessionServiceMock = new Mock<ISessionService>();

            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ReturnsAsync(new UserModel
                {
                    id = 1,
                    username = string.Empty,
                    email = string.Empty,
                    password = string.Empty,
                    is_blocked = false,
                    is_2fa_enabled = true
                });
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(123);
            sessionServiceMock.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(SmtpClientException)));

            var sessionController = new AuthSessionController(sessionServiceMock.Object, userRepositoryMock.Object,
                passwordManagerMock.Object, null, generateMock.Object);

            var result = await sessionController.Login(new AuthDTO { email = string.Empty, password = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task VerifyTwoFA_Success()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var sessionServiceMock = new Mock<ISessionService>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            sessionServiceMock.Setup(x => x.GetData(It.IsAny<string>()))
                .ReturnsAsync(new UserContextObject { Code = string.Empty, UserId = 1 });
            sessionServiceMock.Setup(x => x.CreateTokens(It.IsAny<UserModel>(), It.IsAny<HttpContext>()))
                .Returns(Task.FromResult<IActionResult>(new StatusCodeResult(200)));

            var sessionController = new AuthSessionController(sessionServiceMock.Object, userRepositoryMock.Object,
                passwordManagerMock.Object, null, null);

            var result = await sessionController.VerifyTwoFA(123, string.Empty);

            Assert.Equal(200, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task VerifyTwoFA_UserContextIsNull()
        {
            var sessionServiceMock = new Mock<ISessionService>();

            sessionServiceMock.Setup(x => x.GetData(It.IsAny<string>())).ReturnsAsync((UserContextObject)null);

            var sessionController = new AuthSessionController(sessionServiceMock.Object, null, null, null, null);

            var result = await sessionController.VerifyTwoFA(123, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task VerifyTwoFA_CodeIncorrect()
        {
            var sessionServiceMock = new Mock<ISessionService>();
            var passwordManagerMock = new Mock<IPasswordManager>();

            sessionServiceMock.Setup(x => x.GetData(It.IsAny<string>()))
                .ReturnsAsync(new UserContextObject { Code = string.Empty, UserId = 1});
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var sessionController = new AuthSessionController(sessionServiceMock.Object, null, passwordManagerMock.Object, null, null);

            var result = await sessionController.VerifyTwoFA(123, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(422, objectResult.StatusCode);
        }

        [Fact]
        public async Task VerifyTwoFA_UserNotFound()
        {
            var sessionServiceMock = new Mock<ISessionService>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            sessionServiceMock.Setup(x => x.GetData(It.IsAny<string>()))
                .ReturnsAsync(new UserContextObject { Code = string.Empty, UserId = 1 });
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);

            var sessionController = new AuthSessionController(sessionServiceMock.Object, userRepositoryMock.Object,
                passwordManagerMock.Object, null, null);

            var result = await sessionController.VerifyTwoFA(123, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task VerifyTwoFA_DbConnectionFailed()
        {
            var sessionServiceMock = new Mock<ISessionService>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            sessionServiceMock.Setup(x => x.GetData(It.IsAny<string>()))
                .ReturnsAsync(new UserContextObject { Code = string.Empty, UserId = 1 });
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));

            var sessionController = new AuthSessionController(sessionServiceMock.Object, userRepositoryMock.Object,
                passwordManagerMock.Object, null, null);

            var result = await sessionController.VerifyTwoFA(123, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
