using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using webapi.Controllers.Account.Edit;
using webapi.DB;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Services;
using webapi.Models;

namespace tests.Contollers_Tests.Account.Edit
{
    public class _2FaController_Test
    {
        [Fact]
        public async Task SendCode_Success()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var passwordManagerMock = new Mock<IPasswordManager>();
            var _2faServiceMock = new Mock<IApi2FaService>();
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
            _2faServiceMock.Setup(x => x.SendMessage("air", "air147@mail.com", 111111)).Returns(Task.CompletedTask);
            _2faServiceMock.Setup(x => x.SetSessionCode(null, 111111));
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var _2faController = new _2FaController(userRepositoryMock.Object, _2faServiceMock.Object,
                passwordManagerMock.Object, userInfoMock.Object, generateMock.Object, null);

            var result = await _2faController.SendVerificationCode(string.Empty);

            Assert.Equal(200, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task SendCode_Failed_UserNotFound()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();

            userRepositoryMock.Setup(x => x.GetById(1, CancellationToken.None)).ReturnsAsync((UserModel)null);
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var _2faController = new _2FaController(userRepositoryMock.Object, null, null, userInfoMock.Object, null, null);

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

            userRepositoryMock.Setup(x => x.GetById(1, CancellationToken.None))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(OperationCanceledException)));
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var _2faController = new _2FaController(userRepositoryMock.Object, null, null, userInfoMock.Object, null, null);

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

            var _2faController = new _2FaController(userRepositoryMock.Object, null, passwordManagerMock.Object,
                userInfoMock.Object, null, null);

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
            var _2faServiceMock = new Mock<IApi2FaService>();
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
            _2faServiceMock.Setup(x => x.SendMessage("air", "air147@mail.com", 111111))
                .ThrowsAsync((Exception)Activator.CreateInstance(typeof(SmtpClientException)));

            _2faServiceMock.Setup(x => x.SetSessionCode(null, 111111));
            userInfoMock.Setup(x => x.UserId).Returns(1);

            var _2faController = new _2FaController(userRepositoryMock.Object, _2faServiceMock.Object,
                passwordManagerMock.Object, userInfoMock.Object, generateMock.Object, null);

            var result = await _2faController.SendVerificationCode(string.Empty);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateState_Success()
        {
            var userInfoMock = new Mock<IUserInfo>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var _2faServiceMock = new Mock<IApi2FaService>();
            var validationMock = new Mock<IValidation>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            userRepositoryMock.Setup(x => x.GetById(1, CancellationToken.None)).ReturnsAsync(new UserModel
            {
                id = 1,
            });
            _2faServiceMock.Setup(x => x.GetSessionCode(null)).Returns(1);
            _2faServiceMock.Setup(x => x.ClearData(null, 1)).Returns(Task.CompletedTask);
            _2faServiceMock.Setup(x => x.DbTransaction(null, true)).Returns(Task.CompletedTask);
            validationMock.Setup(x => x.IsSixDigit(It.IsAny<int>())).Returns(true);

            var _2faController = new _2FaController(userRepositoryMock.Object, _2faServiceMock.Object,
                null, userInfoMock.Object, null, validationMock.Object);

            var result = await _2faController.Update2FaState(1, true);

            Assert.Equal(200, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task UpdateState_Failed_InvalidSavedCode()
        {
            var _2faServiceMock = new Mock<IApi2FaService>();
            var validationMock = new Mock<IValidation>();

            _2faServiceMock.Setup(x => x.GetSessionCode(null)).Returns(1);
            validationMock.Setup(x => x.IsSixDigit(It.IsAny<int>())).Returns(false);

            var _2faController = new _2FaController(null, _2faServiceMock.Object,
                null, null, null, validationMock.Object);

            var result = await _2faController.Update2FaState(123456, true);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateState_UserNotFound()
        {
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var userInfoMock = new Mock<IUserInfo>();
            var _2faServiceMock = new Mock<IApi2FaService>();
            var validationMock = new Mock<IValidation>();

            userRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync((UserModel)null);
            userInfoMock.Setup(x => x.UserId).Returns(1);
            _2faServiceMock.Setup(x => x.GetSessionCode(null)).Returns(123);
            validationMock.Setup(x => x.IsSixDigit(It.IsAny<int>())).Returns(true);

            var _2faController = new _2FaController(userRepositoryMock.Object, _2faServiceMock.Object,
                null, userInfoMock.Object, null, validationMock.Object);

            var result = await _2faController.Update2FaState(123, true);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Theory]
        [InlineData(typeof(EntityNotCreatedException))]
        [InlineData(typeof(EntityNotUpdatedException))]
        public async Task UpdateState_DbOpetationFailed(Type exType)
        {
            var userInfoMock = new Mock<IUserInfo>();
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var _2faServiceMock = new Mock<IApi2FaService>();
            var validationMock = new Mock<IValidation>();

            userInfoMock.Setup(x => x.UserId).Returns(1);
            userRepositoryMock.Setup(x => x.GetById(1, CancellationToken.None)).ReturnsAsync(new UserModel
            {
                id = 1,
                username = "air",
                email = "air147@mail.com",
                password = "AirPassword"
            });

            validationMock.Setup(x => x.IsSixDigit(It.IsAny<int>())).Returns(true);
            _2faServiceMock.Setup(x => x.GetSessionCode(null)).Returns(123456);
            _2faServiceMock.Setup(x => x.DbTransaction(It.IsAny<UserModel>(), true))
                .ThrowsAsync((Exception)Activator.CreateInstance(exType));

            var _2faController = new _2FaController(userRepositoryMock.Object, _2faServiceMock.Object,
                null, userInfoMock.Object, null, validationMock.Object);

            var result = await _2faController.Update2FaState(123456, true);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task DbTransaction_Success()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var userRepositoryMock = new Mock<IRepository<UserModel>>();
            var notificationRepositoryMock = new Mock<IRepository<NotificationModel>>();

            var _2faService = new _2FaService(userRepositoryMock.Object, notificationRepositoryMock.Object, context, null, null);

            var userModel = new UserModel
            {
                is_2fa_enabled = false
            };

            var result = _2faService.DbTransaction(userModel, true);

            Assert.True(result.IsCompleted);
        }
    }
}
