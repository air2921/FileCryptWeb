using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Account;
using webapi.DB.Abstractions;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers.Abstractions;
using webapi.Third_Party_Services.Abstractions;
using webapi.Models;
using webapi.Services.Abstractions;
using webapi.Services.Account;
using webapi.DB.Ef.Specifications;

namespace tests.Controllers_Tests.Account
{
    public class SessionController_Test
    {
        [Fact]
        public async Task Login_2FaDisable_Success()
        {
            var inputPassword = "password";
            var inputEmail = "air23663@gmail.com";
            var password = "password";
            var email = "air23663@gmail.com";
            var user = new UserModel
            {
                id = 1,
                username = string.Empty,
                email = email,
                password = password,
                is_blocked = false,
                is_2fa_enabled = false
            };

            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var sessionServiceMock = new Mock<ISessionHelpers>();
            var dataManagementMock = new Mock<IDataManagement>();

            userRepositoryMock.Setup(x => x.GetByFilter(It.Is<UserByEmailSpec>(x => x.Email == inputEmail), CancellationToken.None))
                .ReturnsAsync(user);
            passwordManagerMock.Setup(x => x.CheckPassword(inputPassword, password)).Returns(true);
            sessionServiceMock.Setup(x => x.CreateTokens(user, It.IsAny<HttpContext>()))
                .Returns(Task.FromResult<IActionResult>(new StatusCodeResult(200)));

            var sessionController = new AuthSessionController(sessionServiceMock.Object, dataManagementMock.Object,
                userRepositoryMock.Object, null, passwordManagerMock.Object, null, null);

            var result = await sessionController.Login(new AuthDTO { email = inputEmail, password = inputPassword });

            Assert.Equal(200, ((StatusCodeResult)result).StatusCode);

            sessionServiceMock.Verify(x => x.CreateTokens(user, It.IsAny<HttpContext>()), Times.Once);
        }

        [Fact]
        public async Task Login_2FaEnable_Success()
        {
            var inputPassword = "password";
            var inputEmail = "air23663@gmail.com";
            var id = 1;
            var password = "password";
            var email = "air23663@gmail.com";
            var username = "air2921";
            var code = 123456;
            var hashCode = "hash_Code";
            var user = new UserModel
            {
                id = id,
                username = username,
                email = email,
                password = password,
                is_blocked = false,
                is_2fa_enabled = true
            };

            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var generateMock = new Mock<IGenerate>();
            var dataManagementMock = new Mock<IDataManagement>();
            var emailSenderMock = new Mock<IEmailSender>();

            userRepositoryMock.Setup(x => x.GetByFilter(It.Is<UserByEmailSpec>(x => x.Email == inputEmail), CancellationToken.None))
                .ReturnsAsync(user);
            passwordManagerMock.Setup(x => x.CheckPassword(inputPassword, password)).Returns(true);
            passwordManagerMock.Setup(x => x.HashingPassword(code.ToString())).Returns(hashCode);
            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(code);

            var sessionController = new AuthSessionController(null, dataManagementMock.Object,
                userRepositoryMock.Object, emailSenderMock.Object, passwordManagerMock.Object, null, generateMock.Object);

            var result = await sessionController.Login(new AuthDTO { email = inputEmail, password = inputPassword });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);

            dataManagementMock.Verify(x => x.SetData($"AuthSessionController_UserObject_Email:{email}",
                It.Is<UserContextObject>(u => u.UserId == id && u.Code == hashCode)), Times.Once);
            emailSenderMock.Verify(x => x.SendMessage(It.Is<EmailDto>(e => e.username == username && e.email == email)), Times.Once);
        }

        [Fact]
        public async Task Login_UserNotFound()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<UserByEmailSpec>(), CancellationToken.None))
                .ReturnsAsync((UserModel)null);

            var sessionController = new AuthSessionController(null, null, userRepositoryMock.Object, null, null, null, null);

            var result = await sessionController.Login(new AuthDTO { email = string.Empty, password = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task Login_DbConnectionFailed()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<UserByEmailSpec>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            var sessionController = new AuthSessionController(null, null, userRepositoryMock.Object, null, null, null, null);

            var result = await sessionController.Login(new AuthDTO { email = string.Empty, password = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task Login_UserBlocked()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<UserByEmailSpec>(), CancellationToken.None))
                .ReturnsAsync(new UserModel
                {
                    id = 1,
                    is_blocked = true
                });

            var sessionController = new AuthSessionController(null, null, userRepositoryMock.Object, null, null, null, null);

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
            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<UserByEmailSpec>(), CancellationToken.None))
                .ReturnsAsync(new UserModel
                {
                    id = 1,
                    is_blocked = false
                });

            var sessionController = new AuthSessionController(null, null, userRepositoryMock.Object, null,
                passwordManagerMock.Object, null, null);

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
            var emailSenderMock = new Mock<IEmailSender>();

            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<UserByEmailSpec>(), CancellationToken.None))
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
            emailSenderMock.Setup(x => x.SendMessage(It.IsAny<EmailDto>()))
                .ThrowsAsync(new SmtpClientException());

            var sessionController = new AuthSessionController(null, null, userRepositoryMock.Object, emailSenderMock.Object,
                passwordManagerMock.Object, null, generateMock.Object);

            var result = await sessionController.Login(new AuthDTO { email = string.Empty, password = string.Empty });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task VerifyTwoFA_Success()
        {
            var id = 1;
            var email = "air23663@gmail.com";
            var inputEmail = "air23663@gmail.com";
            var hashCode = "hash_Code";
            var code = 123456;
            var user = new UserModel { email = email };

            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var sessionServiceMock = new Mock<ISessionHelpers>();
            var dataManagementMock = new Mock<IDataManagement>();

            userRepositoryMock.Setup(x => x.GetById(id, CancellationToken.None)).ReturnsAsync(user);
            passwordManagerMock.Setup(x => x.CheckPassword(code.ToString(), hashCode)).Returns(true);
            dataManagementMock.Setup(x => x.GetData($"AuthSessionController_UserObject_Email:{inputEmail}"))
                .ReturnsAsync(new UserContextObject { Code = hashCode, UserId = id });
            sessionServiceMock.Setup(x => x.CreateTokens(user, It.IsAny<HttpContext>()))
                .Returns(Task.FromResult<IActionResult>(new StatusCodeResult(200)));

            var sessionController = new AuthSessionController(sessionServiceMock.Object, dataManagementMock.Object,
                userRepositoryMock.Object, null, passwordManagerMock.Object, null, null);

            var result = await sessionController.VerifyTwoFA(code, inputEmail);

            Assert.Equal(200, ((StatusCodeResult)result).StatusCode);

            sessionServiceMock.Verify(x => x.CreateTokens(user, It.IsAny<HttpContext>()), Times.Once);
        }

        [Fact]
        public async Task VerifyTwoFA_UserContextIsNull()
        {
            var dataManagementMock = new Mock<IDataManagement>();

            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>())).ReturnsAsync((UserContextObject)null);

            var sessionController = new AuthSessionController(null, dataManagementMock.Object, null, null, null, null, null);

            var result = await sessionController.VerifyTwoFA(123, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task VerifyTwoFA_CodeIncorrect()
        {
            var dataManagementMock = new Mock<IDataManagement>();
            var passwordManagerMock = new Mock<IPasswordManager>();

            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>()))
                .ReturnsAsync(new UserContextObject { Code = string.Empty, UserId = 1 });
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var sessionController = new AuthSessionController(null, dataManagementMock.Object, null,
                null, passwordManagerMock.Object, null, null);

            var result = await sessionController.VerifyTwoFA(123, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(422, objectResult.StatusCode);
        }

        [Fact]
        public async Task VerifyTwoFA_UserNotFound()
        {
            var dataManagementMock = new Mock<IDataManagement>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>()))
                .ReturnsAsync(new UserContextObject { Code = string.Empty, UserId = 1 });
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);

            var sessionController = new AuthSessionController(null, dataManagementMock.Object, userRepositoryMock.Object,
                null, passwordManagerMock.Object, null, null);

            var result = await sessionController.VerifyTwoFA(123, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task VerifyTwoFA_DbConnectionFailed()
        {
            var dataManagementMock = new Mock<IDataManagement>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>()))
                .ReturnsAsync(new UserContextObject { Code = string.Empty, UserId = 1 });
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            var sessionController = new AuthSessionController(null, dataManagementMock.Object, userRepositoryMock.Object,
                null, passwordManagerMock.Object, null, null);

            var result = await sessionController.VerifyTwoFA(123, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task Logout_Success()
        {
            var sessionServiceMock = new Mock<ISessionHelpers>();
            var tokenServiceMock = new Mock<ITokenService>();

            var sessionController = new AuthSessionController(sessionServiceMock.Object, null, null,
                null, null, tokenServiceMock.Object, null);

            var result = await sessionController.Logout();

            Assert.Equal(200, ((StatusCodeResult)result).StatusCode);

            sessionServiceMock.Verify(x => x.RevokeToken(It.IsAny<HttpContext>()), Times.Once);
            tokenServiceMock.Verify(x => x.DeleteTokens(), Times.Once);
        }

        [Theory]
        [InlineData(typeof(EntityNotDeletedException))]
        public async Task Logout_ThrowsException(Type ex)
        {
            var sessionServiceMock = new Mock<ISessionHelpers>();
            var tokenServiceMock = new Mock<ITokenService>();

            sessionServiceMock.Setup(x => x.RevokeToken(It.IsAny<HttpContext>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(ex));

            var sessionController = new AuthSessionController(sessionServiceMock.Object, null, null,
                null, null, tokenServiceMock.Object, null);

            var result = await sessionController.Logout();

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);

            tokenServiceMock.Verify(x => x.DeleteTokens(), Times.Never);
        }
    }
}
