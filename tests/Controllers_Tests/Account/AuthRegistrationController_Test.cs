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
    public class AuthRegistrationController_Test
    {
        [Fact]
        public async Task Registration_Success()
        {
            var name = "air2921";
            var email = "Air23663@gmail.com";
            var password = "password";
            var _2fa = false;
            var code = 123456;

            var dto = new RegisterDTO
            {
                email = email,
                username = name,
                password = password,
                is_2fa_enabled = _2fa,
            };

            var generateMock = new Mock<IGenerate>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var emailSenderMock = new Mock<IEmailSender>();
            var dataManagementMock = new Mock<IDataManagement>();
            var validatorMock = new Mock<IValidator>();

            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(code);
            userRepositoryMock.Setup(x => x.GetByFilter(new UserByEmailSpec(email), CancellationToken.None))
                .ReturnsAsync((UserModel)null);
            validatorMock.Setup(x => x.IsValid(dto, null)).Returns(true);
            
            var registationController = new AuthRegistrationController(null, dataManagementMock.Object, validatorMock.Object,
                userRepositoryMock.Object, emailSenderMock.Object, null, generateMock.Object);

            var result = await registationController.Registration(dto);

            emailSenderMock.Verify(x => x.SendMessage(It.Is<EmailDto>(e => e.email == email.ToLowerInvariant() && e.username == name)), Times.Once);
            dataManagementMock.Verify(x => x.SetData($"AuthRegistrationController_UserObject_Email:{email.ToLowerInvariant()}",
                It.Is<User>(u => u.Username == name && u.Email == email.ToLowerInvariant()
                && u.Flag2Fa == _2fa && u.Password == password && u.Code == code.ToString() && u.Role == Role.User.ToString())), Times.Once);
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
                .ThrowsAsync(new OperationCanceledException());
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
                .ThrowsAsync(new SmtpClientException());

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
            var inputCode = 123456;
            var inputEmail = "Air23663@gmail.com";

            var userObj = new User
            {
                Code = "hashCode_123456",
                Username = "air2921",
                Email = "air23663@gmail.com",
                Password = "hashPassword",
                Flag2Fa = false,
                Role = Role.User.ToString()
            };

            var passwordManagerMock = new Mock<IPasswordManager>();
            var transactionMock = new Mock<ITransaction<User>>();
            var dataManagementMock = new Mock<IDataManagement>();

            dataManagementMock.Setup(x => x.GetData($"AuthRegistrationController_UserObject_Email:{inputEmail.ToLowerInvariant()}"))
                .ReturnsAsync(userObj);
            passwordManagerMock.Setup(x => x.CheckPassword(inputCode.ToString(), userObj.Code)).Returns(true);

            var registationController = new AuthRegistrationController(transactionMock.Object, dataManagementMock.Object, null,
                null, null, passwordManagerMock.Object, null);

            var result = await registationController.VerifyAccount(inputCode, inputEmail);

            Assert.Equal(201, ((StatusCodeResult)result).StatusCode);

            transactionMock.Verify(x => x.CreateTransaction(userObj, null), Times.Once);
        }

        [Fact]
        public async Task VerifyAccount_UserDataIsNull()
        {
            var dataManagementMock = new Mock<IDataManagement>();

            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>())).ReturnsAsync((User)null);

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

            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>())).ReturnsAsync(new User
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
            var transactionMock = new Mock<ITransaction<User>>();
            var dataManagementMock = new Mock<IDataManagement>();

            dataManagementMock.Setup(x => x.GetData(It.IsAny<string>())).ReturnsAsync(new User
            {
                Code = string.Empty
            });
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            transactionMock.Setup(x => x.CreateTransaction(It.IsAny<User>(), null))
                .ThrowsAsync(new EntityNotCreatedException());

            var registationController = new AuthRegistrationController(transactionMock.Object, dataManagementMock.Object, null,
                null, null, passwordManagerMock.Object, null);

            var result = await registationController.VerifyAccount(123, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
