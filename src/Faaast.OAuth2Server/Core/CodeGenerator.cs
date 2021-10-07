using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Faaast.Authentication.OAuth2Server.Core
{
    public static class CodeGenerator
    {
        private static readonly Random rnd = new Random();
        private static char[] AllowedChars { get; set; }

        public static Func<int, string> GenerateRandomCode { get; set; } = GenerateRandomCodeImpl;

        static CodeGenerator()
        {
            List<char> table = new List<char>();
            foreach (var intChar in Enumerable.Range((int)'a', 26))
            {
                table.Add((char)intChar);
            }

            foreach (var intChar in Enumerable.Range((int)'A', 26))
            {
                table.Add((char)intChar);
            }

            foreach (var intChar in Enumerable.Range((int)'0', 10))
            {
                table.Add((char)intChar);
            }
            AllowedChars = table.ToArray();
        }

        private static string GenerateRandomCodeImpl(int length)
        {
            return string.Concat(Enumerable.Range(0, length).Select(x => AllowedChars[rnd.Next(0, AllowedChars.Length)]).ToArray());
        }

        public static string GenerateRandomNumber(int length)
        {
            var randomNumber = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
