using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Faaast.OAuth2Server.Abstraction;
using Faaast.OAuth2Server.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Faaast.OAuth2Server.Core
{
    public class ResourceOwnerPasswordCredentialsFlow : OAuthMiddleware
    {
        public ResourceOwnerPasswordCredentialsFlow(RequestDelegate next, OAuthServerOptions options, ILoggerFactory loggerFactory, ISystemClock clock) : base(next, options, loggerFactory, clock)
        {
        }

        protected override bool ShouldHandle(RequestContext context) => HttpMethods.IsPost(context.HttpContext.Request.Method) && string.Equals(Parameters.Password.ParameterName, context.Read(Parameters.GrantType));

        protected override async Task<RequestResult<string>> HandleAsync(RequestContext context, IOauthServerProvider provider)
        {
            var result = new RequestResult<string>(context);
            var client = await provider.GetClientAsync(context.Require(Parameters.ClientId));
            if (client is null || !client.ClientSecret.Equals(context.Require(Parameters.ClientSecret)))
            {
                return await result.RejectAsync(Resources.Msg_InvalidClient);
            }

            if (!client.IsAllowedFlow(nameof(ResourceOwnerPasswordCredentialsFlow), context))
            {
                return await result.RejectAsync(Resources.Msg_ForbiddenFlow);
            }

            var audience = context.Read(Parameters.Audience);
            if (!client.IsAllowedAudience(audience, context))
            {
                return await result.RejectAsync(Resources.Msg_InvalidAudience);
            }

            var authResult = await client.PasswordSignInAsync(context.Require(Parameters.UserName), context.Require(Parameters.Password), context);
            if (authResult?.Result == null)
            {
                return await result.RejectAsync(authResult?.Error ?? Resources.Msg_LoginFailed, StatusCodes.Status403Forbidden);
            }

            var scope = context.Read(Parameters.Scope);
            if (!client.IsAllowedScope(scope, authResult.Result, context))
            {
                return await result.RejectAsync(Resources.Msg_InvalidScope);
            }

            authResult.Result.AddClaim(new Claim("scope", scope));

            var ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(new System.Security.Claims.ClaimsPrincipal(authResult.Result), "Default");
            Token token = new()
            {
                AccessToken = this.CreateJwtToken(context, client, audience, ticket),
                AccessTokenExpiresUtc = this.Clock.UtcNow.UtcDateTime + this.Options.AccessTokenExpireTimeSpan,
                NameIdentifier = ticket.Principal.FindFirst(ClaimTypes.NameIdentifier).Value,
                RefreshToken = CodeGenerator.GenerateRandomNumber(32),
                RefreshTokenExpiresUtc = this.Clock.UtcNow.UtcDateTime + this.Options.RefreshTokenExpireTimeSpan
            };

            await provider.StoreAsync(token);
            return await result.Success(this.CreateJwtResponse(token));
        }
    }
}
