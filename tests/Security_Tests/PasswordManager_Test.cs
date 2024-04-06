using Microsoft.Extensions.Logging;
using webapi.Helpers.Security;

namespace tests.Security_Tests
{
    public class PasswordManager_Test
    {
        [Fact]
        public void HashingPassword_ReturnsHashedPassword()
        {
            var loggerMock = new Mock<ILogger<PasswordManager>>();
            var passwordManager = new PasswordManager(loggerMock.Object);
            var password = "password123";

            var hashedPassword = passwordManager.HashingPassword(password);

            Assert.NotNull(hashedPassword);
            Assert.NotEmpty(hashedPassword);
        }

        [Fact]
        public void CheckPassword_CorrectPassword_ReturnsTrue()
        {
            var loggerMock = new Mock<ILogger<PasswordManager>>();
            var passwordManager = new PasswordManager(loggerMock.Object);
            var correctPassword = "password123";
            var inputPassword = "password123";

            var result = passwordManager.CheckPassword(inputPassword, passwordManager.HashingPassword(correctPassword));

            Assert.True(result);
        }

        [Fact]
        public void CheckPassword_IncorrectPassword_ReturnsFalse()
        {
            var loggerMock = new Mock<ILogger<PasswordManager>>();
            var passwordManager = new PasswordManager(loggerMock.Object);
            var correctPassword = "password123";
            var inputPassword = "wrongpassword";

            var result = passwordManager.CheckPassword(inputPassword, passwordManager.HashingPassword(correctPassword));

            Assert.False(result);
        }
    }
}
