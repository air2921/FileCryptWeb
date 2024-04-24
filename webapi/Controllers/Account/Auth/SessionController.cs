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
    public class SessionController(ISessionService service) : ControllerBase
    {
        [HttpPost("login")]
        [ValidateAntiForgeryToken]
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
                return StatusCode(200, new
                {
                    access = new
                    {
                        jwt = cookies.Jwt,
                        jwtExpires = GetExpires(ImmutableData.JwtExpiry),
                        refresh = cookies.Refresh,
                        refreshExpires = GetExpires(ImmutableData.RefreshExpiry)
                    }
                });
            }

            return StatusCode(response.Status, new { message = response.Message });
        }

        [HttpPost("verify/2fa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify([FromQuery] string email, [FromQuery] int code)
        {
            var response = await service.Verify2Fa(code, email);
            if (response.Status == 201)
            {
                if (response.ObjectData is not CredentialsDTO cookies)
                    return StatusCode(500, new { message = Message.ERROR });

                SetCredentials(cookies);
                return StatusCode(200, new 
                {
                    access = new
                    {
                        jwt = cookies.Jwt,
                        jwtExpires = GetExpires(ImmutableData.JwtExpiry),
                        refresh = cookies.Refresh,
                        refreshExpires = GetExpires(ImmutableData.RefreshExpiry)
                    }
                });
            }

            return StatusCode(response.Status, new { message = response.Message });
        }

        [HttpDelete("logout")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                if (!HttpContext.Request.Cookies.TryGetValue(ImmutableData.REFRESH_COOKIE_KEY, out string? token))
                    return StatusCode(200);

                var response = await service.Logout(token);
                return StatusCode(response.Status);
            }
            finally
            {
                HttpContext.Response.Cookies.Delete(ImmutableData.JWT_COOKIE_KEY);
                HttpContext.Response.Cookies.Delete(ImmutableData.IS_AUTHORIZED);
            }
        }

        [HttpPost("refresh")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refresh(
            [FromHeader(Name = ImmutableData.REFRESH_TOKEN_HEADER_NAME)] string? token, [FromQuery] bool fromHeader)
        {
            token = GetRefresh(token, fromHeader);
            if (token is null)
                return StatusCode(401, new { message = Message.UNAUTHORIZED });

            var response = await service.UpdateJwt(token);
            if (!response.IsSuccess)
            {
                if (response.Status.Equals(401))
                    HttpContext.Response.Cookies.Delete(ImmutableData.REFRESH_COOKIE_KEY);
                return StatusCode(response.Status, new { message = response.Message });
            }

            if (response.ObjectData is not string jwt)
                return StatusCode(500, new { message = Message.ERROR });

            HttpContext.Response.Cookies.Append(ImmutableData.IS_AUTHORIZED, true.ToString(), new CookieOptions
            {
                MaxAge = ImmutableData.JwtExpiry,
                Secure = true,
                HttpOnly = false,
                SameSite = SameSiteMode.None,
                IsEssential = false
            });

            return StatusCode(response.Status, new 
            { 
                access = new
                {   jwt,
                    jwtExpires = GetExpires(ImmutableData.JwtExpiry)
                } 
            });
        }

        [HttpGet("check")]
        [Authorize]
        public IActionResult CheckAuth()
        {
            return StatusCode(204);
        }

        private void SetCredentials(CredentialsDTO dto)
        {
            var cookieOptionsUserInfo = new CookieOptions
            {
                MaxAge = ImmutableData.JwtExpiry,
                Secure = true,
                HttpOnly = false,
                SameSite = SameSiteMode.None,
                IsEssential = false
            };

            HttpContext.Response.Cookies.Append(ImmutableData.REFRESH_COOKIE_KEY, dto.Refresh, SetOptions(ImmutableData.RefreshExpiry));
            HttpContext.Response.Cookies.Append(ImmutableData.JWT_COOKIE_KEY, dto.Jwt, SetOptions(ImmutableData.JwtExpiry));
            HttpContext.Response.Cookies.Append(ImmutableData.IS_AUTHORIZED, dto.IsAuth.ToString(), cookieOptionsUserInfo);
            HttpContext.Response.Cookies.Append(ImmutableData.USER_ID_COOKIE_KEY, dto.Id, cookieOptionsUserInfo);
            HttpContext.Response.Cookies.Append(ImmutableData.ROLE_COOKIE_KEY, dto.Role, cookieOptionsUserInfo);
        }

        private int GetExpires(TimeSpan expires)
        {
            return (int)(DateTime.UtcNow + expires - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        private string? GetRefresh(string? token, bool fromHeader)
        {
            if (fromHeader)
            {
                if (string.IsNullOrWhiteSpace(token))
                    return null;
                else
                    return token;
            }
            else
            {
                if (!HttpContext.Request.Cookies.TryGetValue(ImmutableData.REFRESH_COOKIE_KEY, out token))
                    return null;
                else
                    return token;
            }
        }

        private CookieOptions SetOptions(TimeSpan expires)
        {
            return new CookieOptions
            {
                MaxAge = expires,
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                IsEssential = true
            };
        }
    }
}
