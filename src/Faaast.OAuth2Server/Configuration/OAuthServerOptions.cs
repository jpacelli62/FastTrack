using System;
using Microsoft.IdentityModel.Tokens;

namespace Faaast.OAuth2Server.Configuration
{
    public class OAuthServerOptions
    {
        /// <summary>
        /// Should not be activated in production environment
        /// </summary>
        public bool DisplayDetailedErrors { get; set; } = false;

        public string Issuer { get; set; }

        public bool AllowInsecureHttp { get; set; }

        public TimeSpan AccessTokenExpireTimeSpan { get; set; } = TimeSpan.FromMinutes(15);

        public TimeSpan RefreshTokenExpireTimeSpan { get; set; } = TimeSpan.FromDays(1);

        public string TokenEndpointPath { get; set; } = "/oauth/token";

        public string AuthorizeEndpointPath { get; set; } = "/oauth/authorize";

        public string UserConsentPath { get; set; } = "/oauth/user-consent";
    }
}
