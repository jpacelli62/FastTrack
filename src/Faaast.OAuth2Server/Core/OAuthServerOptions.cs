using System;

namespace Faaast.Authentication.OAuth2Server.Core
{
    public class OAuthServerOptions
    {
        public bool DisplayErrors { get; set; } = false;

        public string Issuer { get; set; }

        public bool AllowInsecureHttp { get; set; }

        public TimeSpan AccessTokenExpireTimeSpan { get; set; } = TimeSpan.FromMinutes(15);

        public string TokenEndpointPath { get; set; } = "/oauth/token";

        public string AuthorizeEndpointPath { get; set; } = "/oauth/authorize";

        public string UserEndpointPath { get; set; } = "/oauth/user";

        public string UserConsentPath { get; set; } = "/oauth/user-consent";

        public string LoginPath { get; set; } = "/oauth/login";

        public string LogoutPath { get; set; } = "/oauth/logout";
    }
}
