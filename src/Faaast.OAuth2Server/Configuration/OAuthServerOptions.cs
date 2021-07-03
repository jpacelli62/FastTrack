using Faaast.OAuth2Server.Configuration;
using System;
using System.Collections.Generic;

namespace Faaast.OAuth2Server
{
    public class OAuthServerOptions
    {
        public IEnumerable<ClientCredential> ValidClients { get; set; }

        public bool AllowInsecureHttp { get; set; }
        
        public TimeSpan AccessTokenExpireTimeSpan { get; set; }
        
        public string TokenEndpointPath { get; set; }

        public string AuthorizeEndpointPath { get; set; }
    }
}
