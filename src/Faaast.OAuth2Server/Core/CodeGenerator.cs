using System;
using System.Security.Cryptography;

namespace Faaast.OAuth2Server.Core
{
    public static class CodeGenerator
    {
        public static string GenerateRandomNumber(int length)
        {
            var randomNumber = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
