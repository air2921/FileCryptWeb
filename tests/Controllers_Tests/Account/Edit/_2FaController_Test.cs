using Microsoft.AspNetCore.Mvc;
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
    public class _2FaController_Test
    {
        [Fact]
        public async Task SendCode_Success()
        {
            var id = 1;
            var code = 123456;
            var name = "air2921";
            var email = "air23663@gmail.com";

            var dataManagementMock = new Mock<IDataManagement>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var emailSenderMock = new Mock<IEmailSender>();
            var generateMock = new Mock<IGenerate>();
            var userInfoMock = new Mock<IUserInfo>();

            userRepositoryMock.Setup(x => x.GetById(id, CancellationToken.None)).ReturnsAsync(new UserModel
            {
                id = id,
                username = name,
                email = email,
                password = "password"
            });

            passwordManagerMock.Setup(x => x.CheckPassword("password", "password")).Returns(true);
            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(code);
            userInfoMock.Setup(x => x.UserId).Returns(id);

            var _2faController = new _2FaController(null, dataManagementMock.Object, null, emailSenderMock.Object,
                userRepositoryMock.Object, passwordManagerMock.Object, userInfoMock.Object, generateMock.Object);

            var result = await _2faController.SendVerificationCode("password");

            Assert.Equal(200, ((StatusCodeResult)result).StatusCode);
            dataManagementMock.Verify(dm => dm.SetData($"_2FaController_VerificationCode#{id}", code), Times.Once);
            emailSenderMock.Verify(es => es.SendMessage(It.Is<EmailDto>(e => e.email == email && e.username == name)), Times.Once);
        }

        [Fact]
        public async Task SendCode_Failed_UserNotFound()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var _2faController = new _2FaController(null, null, null, null,
                userRepositoryMock.Object, null, userInfoMock.Object, null);

            var result = await _2faController.SendVerificationCode(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task SendCode_Failed_DbOperationFailed()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var _2faController = new _2FaController(null, null, null, null,
                userRepositoryMock.Object, null, userInfoMock.Object, null);

            var result = await _2faController.SendVerificationCode(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task SendCode_Failed_PasswordIncorrect()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var userInfoMock = new Mock<IUserInfo>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UserModel
            {
                id = 1,
            });

            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var _2faController = new _2FaController(null, null, null, null,
                userRepositoryMock.Object, passwordManagerMock.Object, userInfoMock.Object, null);

            var result = await _2faController.SendVerificationCode(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(401, objectResult.StatusCode);
        }

        [Fact]
        public async Task SendCode_Failed_SmtpException()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var emailSernderMock = new Mock<IEmailSender>();
            var dataManagement = new Mock<IDataManagement>();
            var generateMock = new Mock<IGenerate>();
            var userInfoMock = new Mock<IUserInfo>();

            userRepositoryMock.Setup(x => x.GetById(1, CancellationToken.None)).ReturnsAsync(new UserModel
            {
                id = 1,
                username = "air",
                email = "air147@mail.com",
                password = "AirPassword"
            });

            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(111111);
            emailSernderMock.Setup(x => x.SendMessage(It.IsAny<EmailDto>()))
                .ThrowsAsync(new SmtpClientException());

            dataManagement.Setup(x => x.SetData(It.IsAny<string>(), It.IsAny<int>()));
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var _2faController = new _2FaController(null, dataManagement.Object,
                null, emailSernderMock.Object, userRepositoryMock.Object, passwordManagerMock.Object, userInfoMock.Object, generateMock.Object);

            var result = await _2faController.SendVerificationCode(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateState_Success()
        {
            var id = 1;
            var savedCode = 123456;
            var inputCode = 123456;
            bool enable = true;
            var user = new UserModel
            {
                id = id,
            };

            var userInfoMock = new Mock<IUserInfo>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var dataManagementMock = new Mock<IDataManagement>();
            var transactionMock = new Mock<ITransaction<UserModel>>();
            var validatorMock = new Mock<IValidator>();

            userInfoMock.Setup(x => x.UserId).Returns(id);
            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(user);
            dataManagementMock.Setup(x => x.GetData($"_2FaController_VerificationCode#{id}")).ReturnsAsync(savedCode);
            validatorMock.Setup(x => x.IsValid(savedCode, inputCode)).Returns(true);

            var _2faController = new _2FaController(transactionMock.Object, dataManagementMock.Object, validatorMock.Object,
                null, userRepositoryMock.Object, null, userInfoMock.Object, null);

            var result = await _2faController.Update2FaState(inputCode, enable);

            Assert.Equal(200, ((StatusCodeResult)result).StatusCode);
            dataManagementMock.Verify(dm => dm.DeleteData(id, null), Times.Once);
            transactionMock.Verify(tr => tr.CreateTransaction(user, enable), Times.Once);
        }

        [Fact]
        public async Task UpdateState_Failed_InvalidSavedCode()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var dataManagementMock = new Mock<IDataManagement>();
            var validatorMock = new Mock<IValidator>();

            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>())).ReturnsAsync(1);
            validatorMock.Setup(x => x.IsValid(It.IsAny<int>(), It.IsAny<int>())).Returns(false);
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var _2faController = new _2FaController(null, dataManagementMock.Object, validatorMock.Object, null,
                null, null, userInfoMock.Object, null);

            var result = await _2faController.Update2FaState(It.IsAny<int>(), true);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateState_UserNotFound()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var dataManagementMock = new Mock<IDataManagement>();
            var validatorMock = new Mock<IValidator>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);
            userInfoMock.Setup(x => x.UserId).Returns(1);
            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>())).ReturnsAsync(1);
            validatorMock.Setup(x => x.IsValid(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            var _2faController = new _2FaController(null, dataManagementMock.Object, validatorMock.Object,
                null, userRepositoryMock.Object, null, userInfoMock.Object, null);

            var result = await _2faController.Update2FaState(It.IsAny<int>(), true);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateState_DbConnectFailed()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var dataManagementMock = new Mock<IDataManagement>();
            var validatorMock = new Mock<IValidator>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            userInfoMock.Setup(x => x.UserId).Returns(1);
            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>())).ReturnsAsync(1);
            validatorMock.Setup(x => x.IsValid(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            var _2faController = new _2FaController(null, dataManagementMock.Object, validatorMock.Object,
                null, userRepositoryMock.Object, null, userInfoMock.Object, null);

            var result = await _2faController.Update2FaState(It.IsAny<int>(), true);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Theory]
        [InlineData(typeof(EntityNotCreatedException))]
        [InlineData(typeof(EntityNotUpdatedException))]
        public async Task UpdateState_TransactionFailed(Type exType)
        {
            var userInfoMock = new Mock<IUserInfo>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var transactionMock = new Mock<ITransaction<UserModel>>();
            var dataManagementMock = new Mock<IDataManagement>();
            var validatorMock = new Mock<IValidator>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            userRepositoryMock.Setup(x => x.GetById(1, CancellationToken.None)).ReturnsAsync(new UserModel
            {
                id = 1,
                username = "air",
                email = "air147@mail.com",
                password = "AirPassword"
            });

            validatorMock.Setup(x => x.IsValid(It.IsAny<int>(), It.IsAny<int>())).Returns(true);
            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>())).ReturnsAsync(1);
            transactionMock.Setup(x => x.CreateTransaction(It.IsAny<UserModel>(), true))
                .ThrowsAsync((Exception)Activator.CreateInstance(exType));

            var _2faController = new _2FaController(transactionMock.Object, dataManagementMock.Object, validatorMock.Object,
                null, userRepositoryMock.Object, null, userInfoMock.Object, null);

            var result = await _2faController.Update2FaState(It.IsAny<int>(), true);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
