﻿namespace webapi.Interfaces.Services
{
    public interface IValidation
    {
        public bool IsBase64String(string? key);
        public bool IsSixDigit(int value);
    }
}
