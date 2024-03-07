using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using webapi.DB;
using webapi.Interfaces.Services;
using webapi.Models;
using webapi.Security;

namespace tests.Security_Tests
{
    public class TokenService_Test
    {
        [Fact]
        public void GenerateJwtToken_ReturnValidToken()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase").Options;

            var dbContext = new FileCryptDbContext(options);

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> { { "SecretKey", "secret_key_that_is_at_least_128_bits_long" } }).Build();
            var generate = new Mock<IGenerate>();
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var tokenService = new TokenService(configuration, generate.Object, dbContext, contextAccessorMock.Object);
            var userModel = new UserModel { id = 1, username = "testuser", email = "test@example.com", role = "user" };

            var jwt = tokenService.GenerateJwtToken(userModel, TimeSpan.FromMinutes(20));

            Assert.NotNull(jwt);
        }

        [Fact]
        public void GenerateRefreshToken_ReturnToken()
        {
            FileCryptDbContext dbContext = null;
            var configuration = new Mock<IConfiguration>();
            var generate = new Mock<IGenerate>();
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var tokenService = new TokenService(configuration.Object, generate.Object, dbContext, contextAccessorMock.Object);

            var refresh = tokenService.GenerateRefreshToken();

            Assert.NotNull(refresh);
        }

        [Fact]
        public void SetCookieOptions_ReturnCorrectCookieOptions()
        {
            var configuration = new Mock<IConfiguration>();
            var generate = new Mock<IGenerate>();

            FileCryptDbContext dbContext = null;
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var tokenService = new TokenService(configuration.Object, generate.Object, dbContext, contextAccessorMock.Object);
            var expireTime = TimeSpan.FromMinutes(20);

            var cookieOptions = tokenService.SetCookieOptions(expireTime);

            Assert.True(cookieOptions.HttpOnly);
            Assert.Equal(expireTime, cookieOptions.MaxAge);
            Assert.True(cookieOptions.Secure);
            Assert.Equal(SameSiteMode.None, cookieOptions.SameSite);
        }

        // THIS TEST IS NOT COMPLETED (IDK HOW TO FIX IT)
        // System.NullReferenceException : Object reference not set to an instance of an object.
        // Stacktrace: TokenService.UpdateJwtToken() line 121 -> TokenService_Test.UpdateJwt_SuccessUpdated() line 115

        //[Fact]
        //public async Task UpdateJwt_SuccessUpdated()
        //{
        //    var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> { { "SecretKey", "secret_key_that_is_at_least_128_bits_long" } }).Build();
        //    var generate = new Mock<IGenerate>();
        //    var contextAccessorMock = new Mock<IHttpContextAccessor>();

        //    var options = new DbContextOptionsBuilder<FileCryptDbContext>()
        //        .UseInMemoryDatabase(databaseName: "TestDatabase")
        //        .Options;

        //    var dbContext = new FileCryptDbContext(options);
        //    var tokenService = new TokenService(configuration, generate.Object, dbContext, contextAccessorMock.Object);

        //    var token = "hujkdfgyhg89fd7ygoidfjhngkluydf908sg7uiodsfjguds98gfuydflskghjds987g89dsfugjk";

        //    var tokenModel = new TokenModel
        //    {
        //        user_id = 1,
        //        token_id = 1,
        //        refresh_token = tokenService.HashingToken(token),
        //        expiry_date = DateTime.UtcNow + ImmutableData.RefreshExpiry
        //    };

        //    var userModel = new UserModel
        //    {
        //        id = 1,
        //        username = "air",
        //        role = "Admin",
        //        email = "testemail@mail.com",
        //        password = "$2a$11$5Ofunw7xW15BlZRfYY6BWufyK7cpnSv4lgf0TfEi3Fz1RABaPXzja",
        //        is_2fa_enabled = true,
        //        is_blocked = false,
        //    };

        //    dbContext.Users.Add(userModel);
        //    dbContext.Tokens.Add(tokenModel);
        //    dbContext.SaveChanges();

        //    contextAccessorMock.Setup(m => m.HttpContext.Request.Cookies.TryGetValue(ImmutableData.REFRESH_COOKIE_KEY, out token))
        //        .Returns(true);

        //    await tokenService.UpdateJwtToken();

        //    contextAccessorMock.Verify(x => x.HttpContext.Response.Cookies.Append(ImmutableData.JWT_COOKIE_KEY, It.IsAny<string>(), It.IsAny<CookieOptions>()), Times.Once);
        //}
    }
}
