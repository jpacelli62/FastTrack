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
    public class ResourceOwnerPasswordCredentialsGrantFlow : OAuthMiddleware
    {
        public ResourceOwnerPasswordCredentialsGrantFlow(RequestDelegate next, OAuthServerOptions options, ILoggerFactory loggerFactory, ISystemClock clock) : base(next, options, loggerFactory, clock)
        {
        }

        protected override bool MatchEndpoint(RequestContext context)
        {
            return this.Options.TokenEndpointPath.Equals(context.HttpContext.Request.Path, StringComparison.OrdinalIgnoreCase);
        }

        protected override bool ShouldHandle(RequestContext context) => HttpMethods.IsPost(context.HttpContext.Request.Method) && string.Equals(Parameters.Password.ParameterName, context.Read(Parameters.GrantType));

        protected override async Task<RequestResult<string>> HandleAsync(RequestContext context)
        {
            var result = new RequestResult<string>(context);
            var clientProvider = context.HttpContext.RequestServices.GetRequiredService<IOauthServerProvider>();
            var client = await clientProvider.GetClientAsync(context.Require(Parameters.ClientId));
            if (client is null || !client.ClientSecret.Equals(context.Require(Parameters.ClientSecret)))
            {
                return await result.RejectAsync(Resources.Msg_InvalidClient);
            }

            if (!client.IsAllowedFlow(nameof(ResourceOwnerPasswordCredentialsGrantFlow), context))
            {
                return await result.RejectAsync(Resources.Msg_ForbiddenFlow);
            }

            var audience = context.Read(Parameters.Audience);
            if (!client.IsAllowedAudience(audience, context))
            {
                return await result.RejectAsync(Resources.Msg_InvalidAudience);
            }

            var flowProvider = context.HttpContext.RequestServices.GetRequiredService<IResourceOwnerPasswordProvider>();
            var authResult = await flowProvider.PasswordSignInAsync(context.Require(Parameters.UserName), context.Require(Parameters.Password), client, context);
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
                AccessToken = this.CreateJwtToken(context, client, string.IsNullOrWhiteSpace(audience) ? client.Audience : audience, ticket),
                AccessTokenExpiresUtc = this.Clock.UtcNow.UtcDateTime + this.Options.AccessTokenExpireTimeSpan,
                NameIdentifier = ticket.Principal.FindFirst(ClaimTypes.NameIdentifier).Value
            };

            var refreshTokenProvider = context.HttpContext.RequestServices.GetRequiredService<IRefreshTokenProvider>();
            if(refreshTokenProvider != null)
            {
                token.RefreshToken = CodeGenerator.GenerateRandomNumber(32);
                token.RefreshTokenExpiresUtc = this.Clock.UtcNow.UtcDateTime + this.Options.RefreshTokenExpireTimeSpan;
                await refreshTokenProvider.StoreAsync(token);
            }

            return await result.Success(this.CreateJwtResponse(token));
        }
    }
}
