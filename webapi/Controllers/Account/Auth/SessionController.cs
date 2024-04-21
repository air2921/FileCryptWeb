using application.Abstractions.Endpoints.Account;
using application.DTO.Outer;
using application.Helpers;
using application.Helpers.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace webapi.Controllers.Account.Auth
{
    [Route("api/auth")]
    [ApiController]
    [ValidateAntiForgeryToken]
    public class SessionController(ISessionService service) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var response = await service.Login(dto);
            if (response.Status == 200)
                return StatusCode(response.Status, new { message = response.Message, confirm = response.ObjectData });

            if (response.Status == 201)
            {
                if (response.ObjectData is not CredentialsDTO cookies)
                    return StatusCode(500, new { message = Message.ERROR });

                SetCredentials(cookies);
                var expires = (int)(DateTime.UtcNow + ImmutableData.JwtExpiry - new DateTime(1970, 1, 1)).TotalSeconds;
                return StatusCode(200, new { jwt = cookies.Jwt, expires = expires });
            }

            return StatusCode(response.Status, new { message = response.Message });
        }

        [HttpPost("verify/2fa")]
        public async Task<IActionResult> Verify([FromQuery] string email, [FromQuery] int code)
        {
            var response = await service.Verify2Fa(code, email);
            if (response.Status == 201)
            {
                if (response.ObjectData is not CredentialsDTO cookies)
                    return StatusCode(500, new { message = Message.ERROR });

                SetCredentials(cookies);
                var expires = (int)(DateTime.UtcNow + ImmutableData.JwtExpiry - new DateTime(1970, 1, 1)).TotalSeconds;
                return StatusCode(200, new { jwt = cookies.Jwt, expires = expires });
            }

            return StatusCode(response.Status, new { message = response.Message });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            if (!HttpContext.Request.Cookies.TryGetValue(ImmutableData.REFRESH_COOKIE_KEY, out string token))
                return StatusCode(204);

            var response = await service.Logout(token);
            HttpContext.Response.Headers.Append("X-LOGOUT", true.ToString());
            return StatusCode(response.Status);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (!HttpContext.Request.Cookies.TryGetValue(ImmutableData.REFRESH_COOKIE_KEY, out string token))
                return StatusCode(404, new { message = Message.UNAUTHORIZED});

            var response = await service.UpdateJwt(token);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });

            if (response.ObjectData is not string jwt)
                return StatusCode(500, new { message = Message.ERROR });

            var expires = (int)(DateTime.UtcNow + ImmutableData.JwtExpiry - new DateTime(1970, 1, 1)).TotalSeconds;
            return StatusCode(response.Status, new { token = jwt, expires = expires });
        }

        private void SetCredentials(CredentialsDTO dto)
        {
            var cookieOptionsToken = new CookieOptions
            {
                MaxAge = ImmutableData.RefreshExpiry,
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                IsEssential = true
            };

            var cookieOptionsUserInfo = new CookieOptions
            {
                MaxAge = ImmutableData.JwtExpiry,
                Secure = true,
                HttpOnly = false,
                SameSite = SameSiteMode.None,
                IsEssential = false
            };

            HttpContext.Response.Cookies.Append(ImmutableData.REFRESH_COOKIE_KEY, dto.Refresh, cookieOptionsToken);

            HttpContext.Response.Cookies.Append(ImmutableData.IS_AUTHORIZED, dto.IsAuth.ToString(), cookieOptionsUserInfo);
            HttpContext.Response.Cookies.Append(ImmutableData.USERNAME_COOKIE_KEY, dto.Username, cookieOptionsUserInfo);
            HttpContext.Response.Cookies.Append(ImmutableData.USER_ID_COOKIE_KEY, dto.Id, cookieOptionsUserInfo);
            HttpContext.Response.Cookies.Append(ImmutableData.ROLE_COOKIE_KEY, dto.Role, cookieOptionsUserInfo);
        }
    }
}
