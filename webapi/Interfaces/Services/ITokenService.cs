﻿using webapi.Models;

namespace webapi.Interfaces.Services
{
    public interface ITokenService
    {
        public string GenerateRefreshToken();
        public string GenerateJwtToken(UserModel userModel, int lifetimeInMin);
        public CookieOptions SetCookieOptions(TimeSpan expireTime);
        public Task UpdateJwtToken();
        public string HashingToken(string token);
        public void DeleteTokens();
    }
}
