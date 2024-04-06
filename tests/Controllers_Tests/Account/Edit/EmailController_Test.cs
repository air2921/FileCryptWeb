using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using webapi.Controllers.Account.Edit;
using webapi.DB.Abstractions;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers.Abstractions;
using webapi.Third_Party_Services.Abstractions;
using webapi.Models;
using webapi.Services.Abstractions;

namespace tests.Controllers_Tests.Account.Edit
{
    public class EmailController_Test
    {
        [Fact]
        public async Task StartEmailChangeProcess_Success()
        {
            var id = 1;
            var code = 123456;
            var email = "air23663@gmail.com";
            var name = "air2921";

            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var generateMock = new Mock<IGenerate>();
            var emailSenderMock = new Mock<IEmailSender>();
            var dataManagementMock = new Mock<IDataManagement>();

            userRepositoryMock.Setup(x => x.GetById(id, CancellationToken.None)).ReturnsAsync(new UserModel
            {
                id = id,
                password = "password",
                email = email,
                username = name
            });
            userInfoMock.Setup(x => x.UserId).Returns(1);
            passwordManagerMock.Setup(x => x.CheckPassword("password", "password")).Returns(true);
            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(code);

            var emailController = new EmailController(null, dataManagementMock.Object, null, userRepositoryMock.Object,
                emailSenderMock.Object, passwordManagerMock.Object, generateMock.Object, null, userInfoMock.Object);

            var result = await emailController.StartEmailChangeProcess("password");

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
            emailSenderMock.Verify(es => es.SendMessage(It.Is<EmailDto>(e => e.email == email && e.username == name)), Times.Once);
            dataManagementMock.Verify(es => es.SetData($"EmailController_ConfirmationCode_OldEmail#{id}", code), Times.Once);
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
                .ThrowsAsync(new OperationCanceledException());
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
                .ThrowsAsync(new SmtpClientException());

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
            var id = 1;
            var email = "air2921@gmail.com";
            var name = "air2921";
            var savedCode = 123456;
            var inputCode = 123456;
            var newCode = 654321;

            var generateMock = new Mock<IGenerate>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var emailSenderMock = new Mock<IEmailSender>();
            var dataManagementMock = new Mock<IDataManagement>();
            var validatorMock = new Mock<IValidator>();

            dataManagementMock.Setup(x => x.GetData($"EmailController_ConfirmationCode_OldEmail#{id}")).ReturnsAsync(savedCode);
            validatorMock.Setup(x => x.IsValid(savedCode, inputCode)).Returns(true);
            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(newCode);
            userInfoMock.Setup(x => x.Username).Returns(name);
            userInfoMock.Setup(x => x.UserId).Returns(id);
            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ReturnsAsync((UserModel)null)
                .Callback<Func<IQueryable<UserModel>, IQueryable<UserModel>>, CancellationToken>((query, token) => {
                    var testQuery = new List<UserModel>().AsQueryable();
                    var filteredQuery = query(testQuery);
                    Assert.True(filteredQuery.Expression.ToString().Contains($".email.Equals("));
                });

            var emailController = new EmailController(null, dataManagementMock.Object, validatorMock.Object,
                userRepositoryMock.Object, emailSenderMock.Object, null, generateMock.Object, null, userInfoMock.Object);

            var result = await emailController.ConfirmOldEmail(email, inputCode);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
            dataManagementMock.Verify(dm => dm.SetData($"EmailController_ConfirmationCode_NewEmail#{id}", newCode), Times.Once);
            dataManagementMock.Verify(dm => dm.SetData($"EmailController_Email#{id}", email), Times.Once);
            emailSenderMock.Verify(es => es.SendMessage(It.Is<EmailDto>(e => e.email == email && e.username == name)), Times.Once);
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
                .ThrowsAsync(new OperationCanceledException());
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
                .ThrowsAsync(new SmtpClientException());
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
            var id = 1;
            var savedCode = 123456;
            var inputCode = 123456;
            var email = "email";
            var user = new UserModel();

            var validatorMock = new Mock<IValidator>();
            var dataManagementMock = new Mock<IDataManagement>();
            var transactionMock = new Mock<ITransaction<UserModel>>();
            var tokenServiceMock = new Mock<ITokenService>();
            var userInfoMock = new Mock<IUserInfo>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();

            dataManagementMock.Setup(x => x.GetData($"EmailController_Email#{id}")).ReturnsAsync(email);
            dataManagementMock.Setup(x => x.GetData($"EmailController_ConfirmationCode_NewEmail#{id}")).ReturnsAsync(savedCode);
            validatorMock.Setup(x => x.IsValid(savedCode, inputCode)).Returns(true);
            userInfoMock.Setup(x => x.UserId).Returns(id);
            userRepositoryMock.Setup(x => x.GetById(id, CancellationToken.None)).ReturnsAsync(user);

            var emailController = new EmailController(transactionMock.Object, dataManagementMock.Object, validatorMock.Object,
                userRepositoryMock.Object, null, null, null, tokenServiceMock.Object, userInfoMock.Object);

            var result = await emailController.ConfirmAndUpdateNewEmail(inputCode);

            Assert.Equal(201, ((StatusCodeResult)result).StatusCode);
            transactionMock.Verify(tr => tr.CreateTransaction(user, email), Times.Once);
            tokenServiceMock.Verify(ts => ts.UpdateJwtToken(), Times.Once);
            dataManagementMock.Verify(dm => dm.DeleteData(id, null), Times.Once);
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
                .ThrowsAsync(new OperationCanceledException());

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
                .ThrowsAsync(new OperationCanceledException());

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
            tokenServiceMock.Setup(x => x.UpdateJwtToken())
                .ThrowsAsync(new UnauthorizedAccessException());
            userInfoMock.Setup(x => x.UserId).Returns(1);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel());

            var emailController = new EmailController(transactionMock.Object, dataManagementMock.Object, validatorMock.Object,
                userRepositoryMock.Object, null, null, null, tokenServiceMock.Object, userInfoMock.Object);

            var result = await emailController.ConfirmAndUpdateNewEmail(123456);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(206, objectResult.StatusCode);
            transactionMock.Verify(tr => tr.CreateTransaction(It.IsAny<UserModel>(), It.IsAny<string>()), Times.Once);
            dataManagementMock.Verify(dm => dm.DeleteData(It.IsAny<int>(), null), Times.Once);
            tokenServiceMock.Verify(ts => ts.DeleteTokens(), Times.Once);
        }
    }
}
