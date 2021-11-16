using System;

namespace Faaast.Authentication.OAuth2Server
{
    public class Token
    {
        public string NameIdentifier { get; set; }

        public string RefreshToken { get; set; }

        public DateTime RefreshTokenExpiresUtc { get; set; }

        public string AccessToken { get; set; }

        public DateTime AccessTokenExpiresUtc { get; set; }
    }
}
