using System.Security.Claims;
using System.Text;
using Faaast.OAuth2Server.Abstraction;
using Faaast.OAuth2Server.Core;
using Microsoft.IdentityModel.Tokens;

namespace Faaast.Tests.Authentication.ServerTests
{
    public class TestClient : IClient
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string Audience { get; set; }

        public string Scope { get; set; }

        public SigningCredentials GetSigninCredentials(RequestContext context) => 
            new(new SymmetricSecurityKey(Encoding.Default.GetBytes(this.ClientSecret)), SecurityAlgorithms.HmacSha256);

        public bool IsAllowedAudience(string audience, RequestContext context) =>
            audience == null || audience == this.Audience;

        public bool IsAllowedFlow(string flowName, RequestContext context) => 
            !(context.HttpContext.Request.Headers.TryGetValue(nameof(IsAllowedFlow), out var value) && value == "0");

        public bool IsAllowedRedirectUrl(string redirectUrl, RequestContext context) => 
            !(context.HttpContext.Request.Headers.TryGetValue(nameof(IsAllowedRedirectUrl), out var value) && value == "0");

        public bool IsAllowedScope(string scope, ClaimsIdentity identity, RequestContext context) => 
            scope == this.Scope;
    }
}
