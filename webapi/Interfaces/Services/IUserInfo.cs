﻿namespace webapi.Interfaces.Services
{
    public interface IUserInfo
    {
        public int UserId { get; }
        public string Username { get; }
        public string Role { get; }
        public string Email { get; }
    }
}
