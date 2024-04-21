﻿using application.Master_Services;

namespace application.Abstractions.Endpoints.Account
{
    public interface I2FaService
    {
        public Task<Response> SendVerificationCode(string password, int id);
        public Task<Response> UpdateState(int code, bool enable, int id);
    }
}