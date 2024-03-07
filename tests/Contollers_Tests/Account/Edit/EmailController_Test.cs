using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Account.Edit;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Services;
using webapi.Models;

namespace tests.Contollers_Tests.Account.Edit
{
    public class EmailController_Test
    {
        [Fact]
        public async Task StartEmailChangeProcess_Success()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var emailServiceMock = new Mock<IApiEmailService>();
            var generateMock = new Mock<IGenerate>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel
            {
                id = 1,
                password = It.IsAny<string>(),
            });
            userInfoMock.Setup(x => x.UserId).Returns(1);
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(123456);
            emailServiceMock.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            emailServiceMock.Setup(x => x.SetData(It.IsAny<string>(), It.IsAny<int>()));

            var emailController = new EmailController(emailServiceMock.Object, userRepositoryMock.Object,
                null, passwordManagerMock.Object, generateMock.Object, null, userInfoMock.Object, null);

            var result = await emailController.StartEmailChangeProcess(It.IsAny<string>());

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task StartEmailChangeProcess_UserNotFound()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var emailController = new EmailController(null, userRepositoryMock.Object, null, null, null, null, userInfoMock.Object, null);

            var result = await emailController.StartEmailChangeProcess(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task StartEmailChangeProcess_DbConnectionFailed()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var emailController = new EmailController(null, userRepositoryMock.Object, null, null, null, null, userInfoMock.Object, null);

            var result = await emailController.StartEmailChangeProcess(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task StartEmailChangeProcess_PasswordIncorrect()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var passwordManagerMock = new Mock<IPasswordManager>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel
            {
                id = 1,
                password = "testPassword123"
            });
            userInfoMock.Setup(x => x.UserId).Returns(1);
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var emailController = new EmailController(null, userRepositoryMock.Object, null,
                passwordManagerMock.Object, null, null, userInfoMock.Object, null);

            var result = await emailController.StartEmailChangeProcess("incorrect123");

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(401, objectResult.StatusCode);
        }

        [Fact]
        public async Task StartEmailChangeProcess_EmailSendFailed()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var emailServiceMock = new Mock<IApiEmailService>();
            var generateMock = new Mock<IGenerate>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel
            {
                id = 1,
                password = "testPassword123"
            });
            userInfoMock.Setup(x => x.UserId).Returns(1);
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(123456);
            emailServiceMock.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(SmtpClientException)));

            var emailController = new EmailController(emailServiceMock.Object, userRepositoryMock.Object,
                null, passwordManagerMock.Object, generateMock.Object, null, userInfoMock.Object, null);

            var result = await emailController.StartEmailChangeProcess("incorrect123");

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmOldEmail_Success()
        {
            var emailServiceMock = new Mock<IApiEmailService>();
            var validationMock = new Mock<IValidation>();
            var generateMock = new Mock<IGenerate>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();

            emailServiceMock.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            emailServiceMock.Setup(x => x.GetCode(It.IsAny<string>())).ReturnsAsync(123456);
            emailServiceMock.Setup(x => x.SetData(It.IsAny<string>(), It.IsAny<int>()));
            emailServiceMock.Setup(x => x.SetData(It.IsAny<string>(), It.IsAny<string>()));
            validationMock.Setup(x => x.IsSixDigit(It.IsAny<int>())).Returns(true);
            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(It.IsAny<int>());
            userInfoMock.Setup(x => x.Username).Returns("username");
            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ReturnsAsync((UserModel)null);

            var emailController = new EmailController(emailServiceMock.Object, userRepositoryMock.Object,
                null, null, generateMock.Object, null, userInfoMock.Object, validationMock.Object);

            var result = await emailController.ConfirmOldEmail("TestEmail134@mail.com", 123456);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmOldEmail_InvalidCode()
        {
            var emailServiceMock = new Mock<IApiEmailService>();
            var validationMock = new Mock<IValidation>();
            var userInfoMock = new Mock<IUserInfo>();

            emailServiceMock.Setup(x => x.GetCode(It.IsAny<string>())).ReturnsAsync(123456);
            validationMock.Setup(x => x.IsSixDigit(It.IsAny<int>())).Returns(false);
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var emailController = new EmailController(emailServiceMock.Object, null,
                null, null, null, null, userInfoMock.Object, validationMock.Object);

            var result = await emailController.ConfirmOldEmail("TestEmail134@mail.com", 123);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmOldEmail_UserAlreadyExists()
        {
            var emailServiceMock = new Mock<IApiEmailService>();
            var validationMock = new Mock<IValidation>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();

            emailServiceMock.Setup(x => x.GetCode(It.IsAny<string>())).ReturnsAsync(123456);
            validationMock.Setup(x => x.IsSixDigit(It.IsAny<int>())).Returns(true);
            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ReturnsAsync(new UserModel());
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var emailController = new EmailController(emailServiceMock.Object, userRepositoryMock.Object,
                null, null, null, null, userInfoMock.Object, validationMock.Object);

            var result = await emailController.ConfirmOldEmail("TestEmail134@mail.com", 123456);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(409, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmOldEmail_DbConnectionFailed()
        {
            var emailServiceMock = new Mock<IApiEmailService>();
            var validationMock = new Mock<IValidation>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();

            emailServiceMock.Setup(x => x.GetCode(It.IsAny<string>())).ReturnsAsync(123456);
            validationMock.Setup(x => x.IsSixDigit(It.IsAny<int>())).Returns(true);
            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var emailController = new EmailController(emailServiceMock.Object, userRepositoryMock.Object,
                null, null, null, null, userInfoMock.Object, validationMock.Object);

            var result = await emailController.ConfirmOldEmail("TestEmail134@mail.com", 123456);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmOldEmail_EmailSendFailed()
        {
            var emailServiceMock = new Mock<IApiEmailService>();
            var validationMock = new Mock<IValidation>();
            var generateMock = new Mock<IGenerate>();
            var userInfoMock = new Mock<IUserInfo>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            emailServiceMock.Setup(x => x.GetCode(It.IsAny<string>())).ReturnsAsync(123456);
            emailServiceMock.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(SmtpClientException)));
            validationMock.Setup(x => x.IsSixDigit(It.IsAny<int>())).Returns(true);
            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(It.IsAny<int>());
            userInfoMock.Setup(x => x.Username).Returns("username");
            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ReturnsAsync((UserModel)null);

            var emailController = new EmailController(emailServiceMock.Object, userRepositoryMock.Object,
                null, null, generateMock.Object, null, userInfoMock.Object, validationMock.Object);

            var result = await emailController.ConfirmOldEmail("TestEmail134@mail.com", 123456);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmAndUpdateNewEmail_Success()
        {
            var emailServiceMock = new Mock<IApiEmailService>();
            var validationMock = new Mock<IValidation>();
            var tokenServiceMock = new Mock<ITokenService>();
            var userInfoMock = new Mock<IUserInfo>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            emailServiceMock.Setup(x => x.GetCode(It.IsAny<string>())).ReturnsAsync(123456);
            emailServiceMock.Setup(x => x.GetString(It.IsAny<string>())).ReturnsAsync("TestEmail134@mail.com");
            validationMock.Setup(x => x.IsSixDigit(It.IsAny<int>())).Returns(true);
            tokenServiceMock.Setup(x => x.UpdateJwtToken()).Returns(Task.CompletedTask);
            userInfoMock.Setup(x => x.UserId).Returns(1);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var emailController = new EmailController(emailServiceMock.Object, userRepositoryMock.Object,
                null, null, null, tokenServiceMock.Object, userInfoMock.Object, validationMock.Object);

            var result = await emailController.ConfirmAndUpdateNewEmail(123456);

            Assert.Equal(201, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task ConfirmAndUpdateNewEmail_InvalidCode()
        {
            var emailServiceMock = new Mock<IApiEmailService>();
            var validationMock = new Mock<IValidation>();
            var userInfoMock = new Mock<IUserInfo>();

            emailServiceMock.Setup(x => x.GetCode(It.IsAny<string>())).ReturnsAsync(123);
            validationMock.Setup(x => x.IsSixDigit(It.IsAny<int>())).Returns(false);
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var emailController = new EmailController(emailServiceMock.Object, null, null,
                null, null, null, userInfoMock.Object, validationMock.Object);

            var result = await emailController.ConfirmAndUpdateNewEmail(123);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmAndUpdateNewEmail_SavedEmailNull()
        {
            var emailServiceMock = new Mock<IApiEmailService>();
            var validationMock = new Mock<IValidation>();
            var userInfoMock = new Mock<IUserInfo>();

            emailServiceMock.Setup(x => x.GetCode(It.IsAny<string>())).ReturnsAsync(123);
            emailServiceMock.Setup(x => x.GetString(It.IsAny<string>())).ReturnsAsync((string)null);
            validationMock.Setup(x => x.IsSixDigit(It.IsAny<int>())).Returns(true);
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var emailController = new EmailController(emailServiceMock.Object, null, null, null,
                null, null, userInfoMock.Object, validationMock.Object);

            var result = await emailController.ConfirmAndUpdateNewEmail(123);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmAndUpdateNewEmail_UserNotFound()
        {
            var emailServiceMock = new Mock<IApiEmailService>();
            var validationMock = new Mock<IValidation>();
            var userInfoMock = new Mock<IUserInfo>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            emailServiceMock.Setup(x => x.GetCode(It.IsAny<string>())).ReturnsAsync(123);
            emailServiceMock.Setup(x => x.GetString(It.IsAny<string>())).ReturnsAsync("TestEmail134@mail.com");
            validationMock.Setup(x => x.IsSixDigit(It.IsAny<int>())).Returns(true);
            userInfoMock.Setup(x => x.UserId).Returns(1);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);

            var emailController = new EmailController(emailServiceMock.Object, userRepositoryMock.Object,
                null, null, null, null, userInfoMock.Object, validationMock.Object);

            var result = await emailController.ConfirmAndUpdateNewEmail(123);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmAndUpdateNewEmail_DbConnectionFailed()
        {
            var emailServiceMock = new Mock<IApiEmailService>();
            var validationMock = new Mock<IValidation>();
            var userInfoMock = new Mock<IUserInfo>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            emailServiceMock.Setup(x => x.GetCode(It.IsAny<string>())).ReturnsAsync(123);
            emailServiceMock.Setup(x => x.GetString(It.IsAny<string>())).ReturnsAsync("TestEmail134@mail.com");
            validationMock.Setup(x => x.IsSixDigit(It.IsAny<int>())).Returns(true);
            userInfoMock.Setup(x => x.UserId).Returns(1);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                 .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));

            var emailController = new EmailController(emailServiceMock.Object, userRepositoryMock.Object,
                null, null, null, null, userInfoMock.Object, validationMock.Object);

            var result = await emailController.ConfirmAndUpdateNewEmail(123);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Theory]
        [InlineData(typeof(EntityNotUpdatedException))]
        [InlineData(typeof(EntityNotCreatedException))]
        public async Task ConfirmAndUpdateNewEmail_ThrowsException(Type ex)
        {
            var emailServiceMock = new Mock<IApiEmailService>();
            var validationMock = new Mock<IValidation>();
            var userInfoMock = new Mock<IUserInfo>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            emailServiceMock.Setup(x => x.GetCode(It.IsAny<string>())).ReturnsAsync(123);
            emailServiceMock.Setup(x => x.GetString(It.IsAny<string>())).ReturnsAsync("TestEmail134@mail.com");
            emailServiceMock.Setup(x => x.DbTransaction(It.IsAny<UserModel>(), It.IsAny<string>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(ex));
            validationMock.Setup(x => x.IsSixDigit(It.IsAny<int>())).Returns(true);
            userInfoMock.Setup(x => x.UserId).Returns(1);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var emailController = new EmailController(emailServiceMock.Object, userRepositoryMock.Object,
                null, null, null, null, userInfoMock.Object, validationMock.Object);

            var result = await emailController.ConfirmAndUpdateNewEmail(123);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmAndUpdateNewEmail_JwtUpdateFailed()
        {
            var emailServiceMock = new Mock<IApiEmailService>();
            var validationMock = new Mock<IValidation>();
            var userInfoMock = new Mock<IUserInfo>();
            var tokenServiceMock = new Mock<ITokenService>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            emailServiceMock.Setup(x => x.GetCode(It.IsAny<string>())).ReturnsAsync(123);
            emailServiceMock.Setup(x => x.GetString(It.IsAny<string>())).ReturnsAsync("TestEmail134@mail.com");
            emailServiceMock.Setup(x => x.DbTransaction(It.IsAny<UserModel>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            validationMock.Setup(x => x.IsSixDigit(It.IsAny<int>())).Returns(true);
            userInfoMock.Setup(x => x.UserId).Returns(1);
            tokenServiceMock.Setup(x => x.UpdateJwtToken())
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(UnauthorizedAccessException)));
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var emailController = new EmailController(emailServiceMock.Object, userRepositoryMock.Object,
                null, null, null, tokenServiceMock.Object, userInfoMock.Object, validationMock.Object);

            var result = await emailController.ConfirmAndUpdateNewEmail(123);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(206, objectResult.StatusCode);
        }
    }
}
