﻿using System.Security.Cryptography;
using application.Abstractions.TP_Services;

namespace services.Cryptography.Helpers.Security
{
    public class Generate : IGenerate
    {
        public int GenerateSixDigitCode()
        {
            var random = new Random();

            return random.Next(100000, 999999);
        }

        public string GenerateKey()
        {
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] key = new byte[32];
            rng.GetBytes(key);

            return Convert.ToBase64String(key);
        }
    }
}