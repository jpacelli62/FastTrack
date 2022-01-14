using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Faaast.OAuth2Server.Abstraction;
using Faaast.OAuth2Server.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Faaast.OAuth2Server.Core.Flows
{
    public class ClientCredentialsGrantFlow : OAuthMiddleware
    {
        public ClientCredentialsGrantFlow(RequestDelegate next, OAuthServerOptions options, ILoggerFactory loggerFactory, ISystemClock clock) : base(next, options, loggerFactory, clock)
        {
        }

        protected override bool MatchEndpoint(RequestContext context)
        {
            return this.Options.TokenEndpointPath.Equals(context.HttpContext.Request.Path, StringComparison.OrdinalIgnoreCase);
        }

        protected override bool ShouldHandle(RequestContext context) => HttpMethods.IsPost(context.HttpContext.Request.Method) && string.Equals(Parameters.ClientCredentials, context.Read(Parameters.GrantType));

        protected override async Task<RequestResult<string>> HandleAsync(RequestContext context)
        {
            var result = new RequestResult<string>(context);
            var clientId = context.Require(Parameters.ClientId);
            var clientProvider = context.HttpContext.RequestServices.GetRequiredService<IOauthServerProvider>();
            var client = await clientProvider.GetClientAsync(clientId);
            if (client != null && client.IsAllowedFlow(nameof(ClientCredentialsGrantFlow), context))
            {
                var clientSecret = context.Require(Parameters.ClientSecret);
                if (client.ClientSecret.Equals(clientSecret))
                {
                    var audience = context.Read(Parameters.Audience);
                    if (!client.IsAllowedAudience(audience, context))
                    {
                        return await result.RejectAsync(Resources.Msg_InvalidAudience);
                    }

                    var flowProvider = context.HttpContext.RequestServices.GetRequiredService<IClientCredentialsProvider>();
                    var identity = await flowProvider.CreateClientIdentityAsync(context, client);
                    var ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(new System.Security.Claims.ClaimsPrincipal(identity), "Default");
                    Token token = new()
                    {
                        AccessToken = this.CreateJwtToken(context, client, string.IsNullOrWhiteSpace(audience) ? client.Audience : audience, ticket),
                        AccessTokenExpiresUtc = this.Clock.UtcNow.UtcDateTime + this.Options.AccessTokenExpireTimeSpan,
                        NameIdentifier = ticket.Principal.FindFirst(ClaimTypes.NameIdentifier).Value,
                    };

                    return await result.Success(this.CreateJwtResponse(token));
                }
            }

            return await result.RejectAsync(Resources.Msg_InvalidClient);
        }
    }
}
