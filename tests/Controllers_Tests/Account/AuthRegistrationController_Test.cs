using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Account;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Services;
using webapi.Models;

namespace tests.Controllers_Tests.Account
{
    public class AuthRegistrationController_Test
    {
        [Fact]
        public async Task Registration_Success()
        {
            var generateMock = new Mock<IGenerate>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var registrationServiceMock = new Mock<IApiRegistrationService>();

            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(123456);
            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ReturnsAsync((UserModel)null);
            registrationServiceMock.Setup(x => x.IsValidData(It.IsAny<RegisterDTO>())).Returns(true);

            var registationController = new AuthRegistrationController(userRepositoryMock.Object, registrationServiceMock.Object,
                null, null, null, null, generateMock.Object);

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
            var registrationServiceMock = new Mock<IApiRegistrationService>();

            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(123456);
            registrationServiceMock.Setup(x => x.IsValidData(It.IsAny<RegisterDTO>())).Returns(false);

            var registationController = new AuthRegistrationController(null, registrationServiceMock.Object,
                null, null, null, null, generateMock.Object);

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
            var registrationServiceMock = new Mock<IApiRegistrationService>();

            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(123456);
            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ReturnsAsync(new UserModel());
            registrationServiceMock.Setup(x => x.IsValidData(It.IsAny<RegisterDTO>())).Returns(true);

            var registationController = new AuthRegistrationController(userRepositoryMock.Object, registrationServiceMock.Object,
                null, null, null, null, generateMock.Object);

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
            var registrationServiceMock = new Mock<IApiRegistrationService>();

            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(123456);
            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));
            registrationServiceMock.Setup(x => x.IsValidData(It.IsAny<RegisterDTO>())).Returns(true);

            var registationController = new AuthRegistrationController(userRepositoryMock.Object, registrationServiceMock.Object,
                null, null, null, null, generateMock.Object);

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
            var registrationServiceMock = new Mock<IApiRegistrationService>();

            generateMock.Setup(x => x.GenerateSixDigitCode()).Returns(123456);
            userRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<UserModel>, IQueryable<UserModel>>>(), CancellationToken.None))
                .ReturnsAsync((UserModel)null);
            registrationServiceMock.Setup(x => x.IsValidData(It.IsAny<RegisterDTO>())).Returns(true);
            registrationServiceMock.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(SmtpClientException)));

            var registationController = new AuthRegistrationController(userRepositoryMock.Object, registrationServiceMock.Object,
                null, null, null, null, generateMock.Object);

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
            var registrationServiceMock = new Mock<IApiRegistrationService>();
            var passwordManagerMock = new Mock<IPasswordManager>();

            registrationServiceMock.Setup(x => x.GetUser(It.IsAny<string>())).ReturnsAsync(new UserObject
            {
                Code = string.Empty
            });
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            var registationController = new AuthRegistrationController(null, registrationServiceMock.Object,
                null, passwordManagerMock.Object, null, null, null);

            var result = await registationController.VerifyAccount(123, string.Empty);

            Assert.Equal(201, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task VerifyAccount_UserDataIsNull()
        {
            var registrationServiceMock = new Mock<IApiRegistrationService>();

            registrationServiceMock.Setup(x => x.GetUser(It.IsAny<string>())).ReturnsAsync((UserObject)null);

            var registationController = new AuthRegistrationController(null, registrationServiceMock.Object,
                null, null, null, null, null);

            var result = await registationController.VerifyAccount(123, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task VerifyAccount_CodeIncorrect()
        {
            var registrationServiceMock = new Mock<IApiRegistrationService>();
            var passwordManagerMock = new Mock<IPasswordManager>();

            registrationServiceMock.Setup(x => x.GetUser(It.IsAny<string>())).ReturnsAsync(new UserObject
            {
                Code = string.Empty
            });
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var registationController = new AuthRegistrationController(null, registrationServiceMock.Object,
                null, passwordManagerMock.Object, null, null, null);

            var result = await registationController.VerifyAccount(123, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(422, objectResult.StatusCode);
        }

        [Fact]
        public async Task VerifyAccount_UserNotCreated_ThrowsException()
        {
            var registrationServiceMock = new Mock<IApiRegistrationService>();
            var passwordManagerMock = new Mock<IPasswordManager>();

            registrationServiceMock.Setup(x => x.GetUser(It.IsAny<string>())).ReturnsAsync(new UserObject
            {
                Code = string.Empty
            });
            registrationServiceMock.Setup(x => x.DbTransaction(It.IsAny<UserObject>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(EntityNotCreatedException)));
            passwordManagerMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            var registationController = new AuthRegistrationController(null, registrationServiceMock.Object,
                null, passwordManagerMock.Object, null, null, null);

            var result = await registationController.VerifyAccount(123, string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
