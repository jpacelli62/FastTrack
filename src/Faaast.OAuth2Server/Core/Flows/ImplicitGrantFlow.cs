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
    public class ImplicitGrantFlow : OAuthMiddleware
    {
        public ImplicitGrantFlow(RequestDelegate next, OAuthServerOptions options, ILoggerFactory loggerFactory, ISystemClock clock) : base(next, options, loggerFactory, clock)
        {
        }

        protected override bool MatchEndpoint(RequestContext context) => this.Options.AuthorizeEndpointPath.Equals(context.HttpContext.Request.Path, StringComparison.OrdinalIgnoreCase);

        protected override bool ShouldHandle(RequestContext context) => HttpMethods.IsGet(context.HttpContext.Request.Method) && string.Equals(Parameters.Token, context.Read(Parameters.ResponseType));

        protected override async Task<RequestResult<string>> HandleAsync(RequestContext context)
        {
            var result = new RequestResult<string>(context);
            var clientProvider = context.HttpContext.RequestServices.GetRequiredService<IOauthServerProvider>();
            var client = await clientProvider.GetClientAsync(context.Require(Parameters.ClientId));
            if (client is null)
            {
                return await result.RejectAsync(Resources.Msg_InvalidClient);
            }

            if (!client.IsAllowedFlow(nameof(ImplicitGrantFlow), context))
            {
                return await result.RejectAsync(Resources.Msg_ForbiddenFlow);
            }

            var redirectUri = context.Read(Parameters.RedirectUri);
            if (!client.IsAllowedRedirectUrl(redirectUri, context))
            {
                return await result.RejectAsync(Resources.Msg_InvalidRedirectUri);
            }

            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                var request = context.HttpContext.Request;
                var loginProperties = new AuthenticationProperties()
                {
                    RedirectUri = BuildUri(request.Scheme, request.Host.Host, request.Host.Port, request.Path, request.QueryString.Value)
                };
                await context.HttpContext.ChallengeAsync(loginProperties);
                return await result.Success(null);
            }

            var scope = context.Read(Parameters.Scope);
            if (!client.IsAllowedScope(scope, context.HttpContext.User.Identity as ClaimsIdentity, context))
            {
                return await result.RejectAsync(Resources.Msg_InvalidScope);
            }

            ((ClaimsIdentity)context.HttpContext.User.Identity).AddClaim(new Claim("scope", scope));
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUri,
                IssuedUtc = this.Clock.UtcNow,
                ExpiresUtc = this.Clock.UtcNow + this.Options.AccessTokenExpireTimeSpan
            };
            var ticket = new AuthenticationTicket(context.HttpContext.User, properties, "Default");

            var accessToken = this.CreateJwtToken(context, client, client.Audience, ticket);

            var uri = new Uri(redirectUri);
            var query = string.Format("?access_token={0}&token_type={1}&expires_in={2}&scope={3}&state={4}",
                accessToken,
                "bearer",
                this.Options.AccessTokenExpireTimeSpan.TotalSeconds,
                scope,
                context.Read(Parameters.State));

            var callBackUri = new UriBuilder(uri.Scheme, uri.Host, uri.Port, uri.LocalPath, query);
            context.HttpContext.Response.Redirect(callBackUri.ToString());
            return await result.Success(null);
        }
    }
}