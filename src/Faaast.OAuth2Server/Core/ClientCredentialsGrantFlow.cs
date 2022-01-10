using System.Security.Claims;
using System.Threading.Tasks;
using Faaast.OAuth2Server.Abstraction;
using Faaast.OAuth2Server.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Faaast.OAuth2Server.Core
{
    public class ClientCredentialsGrantFlow : OAuthMiddleware
    {
        public ClientCredentialsGrantFlow(RequestDelegate next, OAuthServerOptions options, ILoggerFactory loggerFactory, ISystemClock clock) : base(next, options, loggerFactory, clock)
        {
        }

        protected override bool ShouldHandle(RequestContext context) => HttpMethods.IsPost(context.HttpContext.Request.Method) && string.Equals(Parameters.ClientCredentials, context.Read(Parameters.GrantType));

        protected override async Task<RequestResult<string>> HandleAsync(RequestContext context, IOauthServerProvider provider)
        {
            var result = new RequestResult<string>(context);
            var clientId = context.Require(Parameters.ClientId);
            var client = await provider.GetClientAsync(clientId);
            if(client != null && client.IsAllowedFlow(nameof(ClientCredentialsGrantFlow), context))
            {
                var clientSecret = context.Require(Parameters.ClientSecret);
                if (client.ClientSecret.Equals(clientSecret))
                {
                    var audience = context.Read(Parameters.Audience);
                    if (client.IsAllowedAudience(audience, context))
                    {
                        var identity = await client.CreateClientIdentityAsync(context);
                        var ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(new System.Security.Claims.ClaimsPrincipal(identity), "Default");
                        Token token = new()
                        {
                            AccessToken = this.CreateJwtToken(context, client, audience, ticket),
                            AccessTokenExpiresUtc = this.Clock.UtcNow.UtcDateTime + this.Options.AccessTokenExpireTimeSpan,
                            NameIdentifier = ticket.Principal.FindFirst(ClaimTypes.NameIdentifier).Value,
                        };

                        return await result.Success(this.CreateJwtResponse(token));
                    }

                    return await result.RejectAsync(Resources.Msg_InvalidAudience);
                }
            }

            return await result.RejectAsync(Resources.Msg_InvalidClient);
        }
    }
}
