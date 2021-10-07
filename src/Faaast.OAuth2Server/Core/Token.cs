using System;

namespace Faaast.Authentication.OAuth2Server.Core
{
    public class Token
    {
        public string RefreshToken { get; set; }

        public DateTime RefreshTokenExpiresUtc { get; set; }

        public string AccessToken { get; set; }

        public DateTime AccessTokenExpiresUtc { get; set; }

        public string NameIdentifier { get; set; }
    }
}
