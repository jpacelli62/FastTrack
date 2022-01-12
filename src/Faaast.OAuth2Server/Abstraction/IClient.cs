using System.Security.Claims;
using Faaast.OAuth2Server.Core;
using Microsoft.IdentityModel.Tokens;

namespace Faaast.OAuth2Server.Abstraction
{
    public interface IClient
    {
        public string ClientId { get; }

        public string ClientSecret { get; }

        public bool IsAllowedScope(string scope, ClaimsIdentity identity, RequestContext context);

        public bool IsAllowedAudience(string audience, RequestContext context);

        public bool IsAllowedFlow(string flowName, RequestContext context);

        public bool IsAllowedRedirectUrl(string redirectUrl, RequestContext context);

        public SigningCredentials GetSigninCredentials(RequestContext context);
    }
}
