using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using webapi.Helpers;

namespace tests.Api
{
    public class UserInfoTest
    {
        [Fact]
        public void Constructor_UserNotAuthenticated_ThrowsException()
        {
            var httpContextAccessorMock = Mock.Of<IHttpContextAccessor>();

            Assert.Throws<InvalidOperationException>(() => new UserData(httpContextAccessorMock));
        }

        [Fact]
        public void UserId_ClaimTypeNotPresent_ThrowsException()
        {
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var claims = new Claim[] { };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            var userData = new UserData(httpContextAccessorMock.Object);

            Assert.Throws<InvalidOperationException>(() => userData.UserId);
        }

        [Fact]
        public void UserId_ValidClaim_ReturnsCorrectValue()
        {
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "123")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            var userData = new UserData(httpContextAccessorMock.Object);

            var result = userData.UserId;

            Assert.Equal(123, result);
        }

        [Theory]
        [InlineData(nameof(UserData.Username))]
        [InlineData(nameof(UserData.Email))]
        [InlineData(nameof(UserData.Role))]
        public void Property_ClaimTypeNotPresent_ThrowsException(string propertyName)
        {
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var claims = new Claim[] { };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            var userData = new UserData(httpContextAccessorMock.Object);

            switch (propertyName)
            {
                case nameof(UserData.Username):
                    Assert.Throws<InvalidOperationException>(() => userData.Username);
                    break;
                case nameof(UserData.Email):
                    Assert.Throws<InvalidOperationException>(() => userData.Email);
                    break;
                case nameof(UserData.Role):
                    Assert.Throws<InvalidOperationException>(() => userData.Role);
                    break;
                default:
                    throw new ArgumentException("Invalid property name", nameof(propertyName));
            }
        }

        [Theory]
        [InlineData(ClaimTypes.Email, "User224@mail.com")]
        [InlineData(ClaimTypes.Role, "Admin")]
        [InlineData(ClaimTypes.Name, "user224")]
        public void Property_ValidClaim_ReturnsCorrectValue(string claimType, string expectedValue)
        {
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var claims = new[]
            {
                new Claim(claimType, expectedValue)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            var userData = new UserData(httpContextAccessorMock.Object);

            string result = null;
            switch (claimType)
            {
                case ClaimTypes.Email:
                    result = userData.Email;
                    break;
                case ClaimTypes.Role:
                    result = userData.Role;
                    break;
                case ClaimTypes.Name:
                    result = userData.Username;
                    break;
            }

            Assert.Equal(expectedValue, result);
        }
    }
}
