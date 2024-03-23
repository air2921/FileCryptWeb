using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Account.Edit;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Services;
using webapi.Models;

namespace tests.Controllers_Tests.Account.Edit
{
    public class EmailController_Test
    {
        [Fact]
        public async Task StartEmailChangeProcess_Success()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var generateMock = new Mock<IGenerate>();
            var emailSenderMock = new Mock<IEmailSender>();
            var dataManagementMock = new Mock<IDataManagement>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel
            {
                id = 1,
                password = It.IsAny<string>(),
            });
            userInfoMock.Setup(x => x.UserId).Returns(1);
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(123456);
            emailSenderMock.Setup(x => x.SendMessage(It.IsAny<EmailDto>())).Returns(Task.CompletedTask);
            dataManagementMock.Setup(x => x.SetData(It.IsAny<string>(), It.IsAny<int>()));

            var emailController = new EmailController(null, dataManagementMock.Object, null, userRepositoryMock.Object,
                emailSenderMock.Object, passwordManagerMock.Object, generateMock.Object, null, userInfoMock.Object);

            var result = await emailController.StartEmailChangeProcess(string.Empty);

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

            var emailController = new EmailController(null, null, null, userRepositoryMock.Object,
                null, null, null, null, userInfoMock.Object);

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

            var emailController = new EmailController(null, null, null, userRepositoryMock.Object,
                null, null, null, null, userInfoMock.Object);

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

            var emailController = new EmailController(null, null, null, userRepositoryMock.Object,
                null, passwordManagerMock.Object, null, null, userInfoMock.Object);

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
            var generateMock = new Mock<IGenerate>();
            var emailSenderMock = new Mock<IEmailSender>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel
            {
                id = 1,
                password = "testPassword123"
            });
            userInfoMock.Setup(x => x.UserId).Returns(1);
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(123456);
            emailSenderMock.Setup(x => x.SendMessage(It.IsAny<EmailDto>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(SmtpClientException)));

            var emailController = new EmailController(null, null, null, userRepositoryMock.Object,
                emailSenderMock.Object, passwordManagerMock.Object, generateMock.Object, null, userInfoMock.Object);

            var result = await emailController.StartEmailChangeProcess("incorrect123");

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmOldEmail_Success()
        {
            var generateMock = new Mock<IGenerate>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var emailSenderMock = new Mock<IEmailSender>();
            var dataManagementMock = new Mock<IDataManagement>();
            var validatorMock = new Mock<IValidator>();

            emailSenderMock.Setup(x => x.SendMessage(It.IsAny<EmailDto>())).Returns(Task.CompletedTask);
            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>())).ReturnsAsync(1);
            dataManagementMock.Setup(x => x.SetData(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.CompletedTask);
            dataManagementMock.Setup(x => x.SetData(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            validatorMock.Setup(x => x.IsValid(It.IsAny<int>(), It.IsAny<int>())).Returns(true);
            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(It.IsAny<int>());
            userInfoMock.Setup(x => x.Username).Returns("username");
            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ReturnsAsync((UserModel)null);

            var emailController = new EmailController(null, dataManagementMock.Object, validatorMock.Object,
                userRepositoryMock.Object, emailSenderMock.Object, null, generateMock.Object, null, userInfoMock.Object);

            var result = await emailController.ConfirmOldEmail("TestEmail134@mail.com", 1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmOldEmail_InvalidCode()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var dataManagementMock = new Mock<IDataManagement>();
            var validatorMock = new Mock<IValidator>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>())).ReturnsAsync(1);
            validatorMock.Setup(x => x.IsValid(It.IsAny<int>(), It.IsAny<int>())).Returns(false);

            var emailController = new EmailController(null, dataManagementMock.Object, validatorMock.Object,
                null, null, null, null, null, userInfoMock.Object);

            var result = await emailController.ConfirmOldEmail("TestEmail134@mail.com", 123);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmOldEmail_UserAlreadyExists()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var validatorMock = new Mock<IValidator>();
            var dataManagementMock = new Mock<IDataManagement>();

            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>())).ReturnsAsync(1);
            validatorMock.Setup(x => x.IsValid(It.IsAny<int>(), It.IsAny<int>())).Returns(true);
            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ReturnsAsync(new UserModel());
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var emailController = new EmailController(null, dataManagementMock.Object, validatorMock.Object,
                userRepositoryMock.Object, null, null, null, null, userInfoMock.Object);

            var result = await emailController.ConfirmOldEmail("TestEmail134@mail.com", 123456);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(409, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmOldEmail_DbConnectionFailed()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var validatorMock = new Mock<IValidator>();
            var dataManagementMock = new Mock<IDataManagement>();

            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>())).ReturnsAsync(1);
            validatorMock.Setup(x => x.IsValid(It.IsAny<int>(), It.IsAny<int>())).Returns(true);
            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var emailController = new EmailController(null, dataManagementMock.Object, validatorMock.Object,
                userRepositoryMock.Object, null, null, null, null, userInfoMock.Object);

            var result = await emailController.ConfirmOldEmail("TestEmail134@mail.com", 123456);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmOldEmail_EmailSendFailed()
        {
            var emailSenderMock = new Mock<IEmailSender>();
            var validatorMock = new Mock<IValidator>();
            var dataManagementMock = new Mock<IDataManagement>();
            var generateMock = new Mock<IGenerate>();
            var userInfoMock = new Mock<IUserInfo>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>())).ReturnsAsync(1);
            emailSenderMock.Setup(x => x.SendMessage(It.IsAny<EmailDto>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(SmtpClientException)));
            validatorMock.Setup(x => x.IsValid(It.IsAny<int>(), It.IsAny<int>())).Returns(true);
            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(It.IsAny<int>());
            userInfoMock.Setup(x => x.Username).Returns("username");
            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ReturnsAsync((UserModel)null);

            var emailController = new EmailController(null, dataManagementMock.Object, validatorMock.Object,
                userRepositoryMock.Object, emailSenderMock.Object, null, generateMock.Object, null, userInfoMock.Object);

            var result = await emailController.ConfirmOldEmail("TestEmail134@mail.com", 123456);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmAndUpdateNewEmail_Success()
        {
            var validatorMock = new Mock<IValidator>();
            var dataManagementMock = new Mock<IDataManagement>();
            var transactionMock = new Mock<ITransaction<UserModel>>();
            var tokenServiceMock = new Mock<ITokenService>();
            var userInfoMock = new Mock<IUserInfo>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            dataManagementMock.Setup(x => x.GetData("EmailController_Email#1")).ReturnsAsync(string.Empty);
            dataManagementMock.Setup(x => x.GetData("EmailController_ConfirmationCode_NewEmail#1")).ReturnsAsync(1);
            validatorMock.Setup(x => x.IsValid(It.IsAny<int>(), It.IsAny<int>())).Returns(true);
            transactionMock.Setup(x => x.CreateTransaction(It.IsAny<UserModel>(), It.IsAny<object>())).Returns(Task.CompletedTask);
            tokenServiceMock.Setup(x => x.UpdateJwtToken()).Returns(Task.CompletedTask);
            userInfoMock.Setup(x => x.UserId).Returns(1);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var emailController = new EmailController(transactionMock.Object, dataManagementMock.Object, validatorMock.Object,
                userRepositoryMock.Object, null, null, null, tokenServiceMock.Object, userInfoMock.Object);

            var result = await emailController.ConfirmAndUpdateNewEmail(123456);

            Assert.Equal(201, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task ConfirmAndUpdateNewEmail_InvalidCode()
        {
            var dataManagementMock = new Mock<IDataManagement>();
            var validatorMock = new Mock<IValidator>();
            var userInfoMock = new Mock<IUserInfo>();

            dataManagementMock.Setup(x => x.GetData("EmailController_Email#1")).ReturnsAsync(string.Empty);
            dataManagementMock.Setup(x => x.GetData("EmailController_ConfirmationCode_NewEmail#1")).ReturnsAsync(null);
            validatorMock.Setup(x => x.IsValid(It.IsAny<int>(), It.IsAny<int>())).Returns(false);
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var emailController = new EmailController(null, dataManagementMock.Object, validatorMock.Object, null,
                null, null, null, null, userInfoMock.Object);

            var result = await emailController.ConfirmAndUpdateNewEmail(123);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmAndUpdateNewEmail_SavedEmailNull()
        {
            var dataManagementMock = new Mock<IDataManagement>();
            var validatorMock = new Mock<IValidator>();
            var userInfoMock = new Mock<IUserInfo>();

            dataManagementMock.Setup(x => x.GetData("EmailController_Email#1")).ReturnsAsync(string.Empty);
            dataManagementMock.Setup(x => x.GetData("EmailController_ConfirmationCode_NewEmail#1")).ReturnsAsync(null);
            validatorMock.Setup(x => x.IsValid(It.IsAny<int>(), It.IsAny<int>())).Returns(false);
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var emailController = new EmailController(null, dataManagementMock.Object, validatorMock.Object,
                null, null, null, null, null, userInfoMock.Object);

            var result = await emailController.ConfirmAndUpdateNewEmail(123);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmAndUpdateNewEmail_UserNotFound()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var dataManagementMock = new Mock<IDataManagement>();
            var validatorMock = new Mock<IValidator>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            dataManagementMock.Setup(x => x.GetData("EmailController_Email#1")).ReturnsAsync(string.Empty);
            dataManagementMock.Setup(x => x.GetData("EmailController_ConfirmationCode_NewEmail#1")).ReturnsAsync(1);
            validatorMock.Setup(x => x.IsValid(It.IsAny<int>(), It.IsAny<int>())).Returns(true);
            userInfoMock.Setup(x => x.UserId).Returns(1);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);

            var emailController = new EmailController(null, dataManagementMock.Object, validatorMock.Object, userRepositoryMock.Object,
                null, null, null, null, userInfoMock.Object);

            var result = await emailController.ConfirmAndUpdateNewEmail(123);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmAndUpdateNewEmail_DbConnectionFailed()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var dataManagementMock = new Mock<IDataManagement>();
            var validatorMock = new Mock<IValidator>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            dataManagementMock.Setup(x => x.GetData("EmailController_Email#1")).ReturnsAsync(string.Empty);
            dataManagementMock.Setup(x => x.GetData("EmailController_ConfirmationCode_NewEmail#1")).ReturnsAsync(1);
            validatorMock.Setup(x => x.IsValid(It.IsAny<int>(), It.IsAny<int>())).Returns(true);
            userInfoMock.Setup(x => x.UserId).Returns(1);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));

            var emailController = new EmailController(null, dataManagementMock.Object, validatorMock.Object, userRepositoryMock.Object,
                null, null, null, null, userInfoMock.Object);

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
            var userInfoMock = new Mock<IUserInfo>();
            var dataManagementMock = new Mock<IDataManagement>();
            var validatorMock = new Mock<IValidator>();
            var transactionMock = new Mock<ITransaction<UserModel>>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            dataManagementMock.Setup(x => x.GetData("EmailController_Email#1")).ReturnsAsync(string.Empty);
            dataManagementMock.Setup(x => x.GetData("EmailController_ConfirmationCode_NewEmail#1")).ReturnsAsync(1);
            validatorMock.Setup(x => x.IsValid(It.IsAny<int>(), It.IsAny<int>())).Returns(true);
            transactionMock.Setup(x => x.CreateTransaction(It.IsAny<UserModel>(), It.IsAny<string>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(ex));
            userInfoMock.Setup(x => x.UserId).Returns(1);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));

            var emailController = new EmailController(transactionMock.Object, dataManagementMock.Object,
                validatorMock.Object, userRepositoryMock.Object, null, null, null, null, userInfoMock.Object);

            var result = await emailController.ConfirmAndUpdateNewEmail(123);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task ConfirmAndUpdateNewEmail_JwtUpdateFailed()
        {
            var validatorMock = new Mock<IValidator>();
            var dataManagementMock = new Mock<IDataManagement>();
            var transactionMock = new Mock<ITransaction<UserModel>>();
            var tokenServiceMock = new Mock<ITokenService>();
            var userInfoMock = new Mock<IUserInfo>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            dataManagementMock.Setup(x => x.GetData("EmailController_Email#1")).ReturnsAsync(string.Empty);
            dataManagementMock.Setup(x => x.GetData("EmailController_ConfirmationCode_NewEmail#1")).ReturnsAsync(1);
            validatorMock.Setup(x => x.IsValid(It.IsAny<int>(), It.IsAny<int>())).Returns(true);
            transactionMock.Setup(x => x.CreateTransaction(It.IsAny<UserModel>(), It.IsAny<object>())).Returns(Task.CompletedTask);
            tokenServiceMock.Setup(x => x.UpdateJwtToken())
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(UnauthorizedAccessException)));
            userInfoMock.Setup(x => x.UserId).Returns(1);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var emailController = new EmailController(transactionMock.Object, dataManagementMock.Object, validatorMock.Object,
                userRepositoryMock.Object, null, null, null, tokenServiceMock.Object, userInfoMock.Object);

            var result = await emailController.ConfirmAndUpdateNewEmail(123456);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(206, objectResult.StatusCode);
        }
    }
}
