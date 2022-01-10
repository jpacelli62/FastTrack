using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Faaast.OAuth2Server.Abstraction;
using Faaast.OAuth2Server.Configuration;
using Faaast.OAuth2Server.Core;
using Microsoft.IdentityModel.Tokens;

namespace Faaast.Tests.Authentication.ServerTests
{
    public class TestClient : IClient
    {
        public string ClientId => "MyClientId";

        public string ClientSecret => "jhgjhvjvkjvklvkjvjkv";

        public static string Audience => "MyAudience";

        public static string Scope => "identity";

        public string Username = "JohnDoe";

        public string Password = "JohnDoePwd";

        public Task<ClaimsIdentity> CreateClientIdentityAsync(RequestContext context)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "-1"),
                new Claim(ClaimTypes.Role, this.ClientId)
            };
            return Task.FromResult(new ClaimsIdentity(claims, "identity.application"));
        }

        public SigningCredentials GetSigninCredentials(RequestContext context) => new(new SymmetricSecurityKey(Encoding.Default.GetBytes(this.ClientSecret)), SecurityAlgorithms.HmacSha256);
        public bool IsAllowedAudience(string audience, RequestContext context) => audience == Audience;
        public bool IsAllowedFlow(string flowName, RequestContext context) => context.Read(Parameters.ClientId)  != "disallowFlow";
        public bool IsAllowedRedirectUrl(string redirectUrl, RequestContext context) => !redirectUrl.Contains("donotredirect");
        public bool IsAllowedScope(string scope, ClaimsIdentity identity, RequestContext context) => scope == Scope;

        public Task<RequestResult<ClaimsIdentity>> PasswordSignInAsync(string username, string password, RequestContext context)
        {
            if(Username.Equals(username) && Password.Equals(password))
            {
                List<Claim> claims = new();
                claims.Add(new Claim(ClaimTypes.Name, "John Doe"));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, "123"));
                RequestResult<ClaimsIdentity> result = new(context);
                return result.Success(new ClaimsIdentity(claims));
            }
            else if (username.Equals("lockedtest"))
            {
                RequestResult<ClaimsIdentity> result = new(context);
                return result.RejectAsync("Locked account");
            }

            return Task.FromResult<RequestResult<ClaimsIdentity>>(null);
        }
    }
}
