using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Account;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Services;
using webapi.Models;
using webapi.Services.Account;

namespace tests.Controllers_Tests.Account
{
    public class AuthRegistrationController_Test
    {
        [Fact]
        public async Task Registration_Success()
        {
            var generateMock = new Mock<IGenerate>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var emailSenderMock = new Mock<IEmailSender>();
            var dataManagementMock = new Mock<IDataManagement>();
            var validatorMock = new Mock<IValidator>();

            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(123456);
            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ReturnsAsync((UserModel)null);
            emailSenderMock.Setup(x => x.SendMessage(It.IsAny<EmailDto>())).Returns(Task.CompletedTask);
            dataManagementMock.Setup(x => x.SetData(It.IsAny<string>(), It.IsAny<UserObject>())).Returns(Task.CompletedTask);
            validatorMock.Setup(x => x.IsValid(It.IsAny<RegisterDTO>(), null)).Returns(true);
            

            var registationController = new AuthRegistrationController(null, dataManagementMock.Object, validatorMock.Object,
                userRepositoryMock.Object, emailSenderMock.Object, null, generateMock.Object);

            var result = await registationController.Registration(new RegisterDTO
            {
                email = "TestUser134@mail.com",
                username = "username",
                password = string.Empty,
                is_2fa_enabled = false,
            });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task Registration_InvalidData()
        {
            var generateMock = new Mock<IGenerate>();
            var validatorMock = new Mock<IValidator>();

            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(123456);
            validatorMock.Setup(x => x.IsValid(It.IsAny<RegisterDTO>(), null)).Returns(false);

            var registationController = new AuthRegistrationController(null, null, validatorMock.Object,
                null, null, null, generateMock.Object);

            var result = await registationController.Registration(new RegisterDTO
            {
                email = "TestUser134@mail.com",
                username = "username",
                password = string.Empty,
                is_2fa_enabled = false,
            });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task Registration_UserAlreadyExists()
        {
            var generateMock = new Mock<IGenerate>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var validatorMock = new Mock<IValidator>();

            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(123456);
            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ReturnsAsync(new UserModel());
            validatorMock.Setup(x => x.IsValid(It.IsAny<RegisterDTO>(), null)).Returns(true);

            var registationController = new AuthRegistrationController(null, null, validatorMock.Object,
                userRepositoryMock.Object, null, null, generateMock.Object);

            var result = await registationController.Registration(new RegisterDTO
            {
                email = "TestUser134@mail.com",
                username = "username",
                password = string.Empty,
                is_2fa_enabled = false,
            });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(409, objectResult.StatusCode);
        }

        [Fact]
        public async Task Registration_DbConnectionFailed()
        {
            var generateMock = new Mock<IGenerate>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var validatorMock = new Mock<IValidator>();

            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(123456);
            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));
            validatorMock.Setup(x => x.IsValid(It.IsAny<RegisterDTO>(), null)).Returns(true);

            var registationController = new AuthRegistrationController(null, null, validatorMock.Object,
                userRepositoryMock.Object, null, null, generateMock.Object);

            var result = await registationController.Registration(new RegisterDTO
            {
                email = "TestUser134@mail.com",
                username = "username",
                password = string.Empty,
                is_2fa_enabled = false,
            });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task Registration_SendEmailFailed()
        {
            var generateMock = new Mock<IGenerate>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var validatorMock = new Mock<IValidator>();
            var emailSenderMock = new Mock<IEmailSender>();

            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(123456);
            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ReturnsAsync((UserModel)null);
            validatorMock.Setup(x => x.IsValid(It.IsAny<RegisterDTO>(), null)).Returns(true);
            emailSenderMock.Setup(x => x.SendMessage(It.IsAny<EmailDto>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(SmtpClientException)));

            var registationController = new AuthRegistrationController(null, null, validatorMock.Object,
                userRepositoryMock.Object, emailSenderMock.Object, null, generateMock.Object);

            var result = await registationController.Registration(new RegisterDTO
            {
                email = "TestUser134@mail.com",
                username = "username",
                password = string.Empty,
                is_2fa_enabled = false,
            });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task VerifyAccount_Success()
        {
            var passwordManagerMock = new Mock<IPasswordManager>();
            var transactionMock = new Mock<ITransaction<UserObject>>();
            var dataManagementMock = new Mock<IDataManagement>();

            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>())).ReturnsAsync(new UserObject
            {
                Code = string.Empty
            });
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            transactionMock.Setup(x => x.CreateTransaction(It.IsAny<UserObject>(), null)).Returns(Task.CompletedTask);

            var registationController = new AuthRegistrationController(transactionMock.Object, dataManagementMock.Object, null,
                null, null, passwordManagerMock.Object, null);

            var result = await registationController.VerifyAccount(123, string.Empty);

            Assert.Equal(201, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task VerifyAccount_UserDataIsNull()
        {
            var dataManagementMock = new Mock<IDataManagement>();

            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>())).ReturnsAsync((UserObject)null);

            var registationController = new AuthRegistrationController(null, dataManagementMock.Object, null,
                null, null, null, null);

            var result = await registationController.VerifyAccount(123, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task VerifyAccount_CodeIncorrect()
        {
            var passwordManagerMock = new Mock<IPasswordManager>();
            var dataManagementMock = new Mock<IDataManagement>();

            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>())).ReturnsAsync(new UserObject
            {
                Code = string.Empty
            });
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var registationController = new AuthRegistrationController(null, dataManagementMock.Object, null,
                null, null, passwordManagerMock.Object, null);

            var result = await registationController.VerifyAccount(123, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(422, objectResult.StatusCode);
        }

        [Fact]
        public async Task VerifyAccount_UserNotCreated_ThrowsException()
        {
            var passwordManagerMock = new Mock<IPasswordManager>();
            var transactionMock = new Mock<ITransaction<UserObject>>();
            var dataManagementMock = new Mock<IDataManagement>();

            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>())).ReturnsAsync(new UserObject
            {
                Code = string.Empty
            });
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            transactionMock.Setup(x => x.CreateTransaction(It.IsAny<UserObject>(), null))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(EntityNotCreatedException)));

            var registationController = new AuthRegistrationController(transactionMock.Object, dataManagementMock.Object, null,
                null, null, passwordManagerMock.Object, null);

            var result = await registationController.VerifyAccount(123, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
