using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Faaast.OAuth2Server.Abstraction;
using Faaast.OAuth2Server.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Faaast.OAuth2Server.Core.Flows
{
    public class AuthorizationCodeGrantFlow : OAuthMiddleware
    {
        public AuthorizationCodeGrantFlow(RequestDelegate next, OAuthServerOptions options, ILoggerFactory loggerFactory, ISystemClock clock) : base(next, options, loggerFactory, clock)
        {
        }

        protected override bool MatchEndpoint(RequestContext context) => this.Options.TokenEndpointPath.Equals(context.HttpContext.Request.Path, StringComparison.OrdinalIgnoreCase) || this.Options.AuthorizeEndpointPath.Equals(context.HttpContext.Request.Path, StringComparison.OrdinalIgnoreCase);

        protected override bool ShouldHandle(RequestContext context) => IsChallenge(context) || IsExchangeCode(context);

        private static bool IsChallenge(RequestContext context) => HttpMethods.IsGet(context.HttpContext.Request.Method) && string.Equals(Parameters.Code.ParameterName, context.Read(Parameters.ResponseType));

        private static bool IsExchangeCode(RequestContext context) => HttpMethods.IsPost(context.HttpContext.Request.Method) && string.Equals(Parameters.AuthorizationCode, context.Read(Parameters.GrantType));

        protected override async Task<RequestResult<string>> HandleAsync(RequestContext context) => IsChallenge(context) ? await this.HandleChallengeAsync(context) : await this.HandleExchangeCodeAsync(context);

        private async Task<RequestResult<string>> HandleChallengeAsync(RequestContext context)
        {
            var result = new RequestResult<string>(context);
            var clientProvider = context.HttpContext.RequestServices.GetRequiredService<IOauthServerProvider>();
            var client = await clientProvider.GetClientAsync(context.Require(Parameters.ClientId));
            if (client is null)
            {
                return await result.RejectAsync(Resources.Msg_InvalidClient);
            }

            if (!client.IsAllowedFlow(nameof(AuthorizationCodeGrantFlow), context))
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

            var code = CodeGenerator.GenerateRandomNumber(32);
            var uri = new Uri(redirectUri);
            var query = string.Format("?code={0}&state={1}", code, System.Web.HttpUtility.UrlEncode(context.Read(Parameters.State)));
            var callBackUri = new UriBuilder(uri.Scheme, uri.Host, uri.Port, uri.LocalPath, query);

            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUri,
                IssuedUtc = this.Clock.UtcNow,
                ExpiresUtc = this.Clock.UtcNow + TimeSpan.FromMinutes(5)
            };

            ((ClaimsIdentity)context.HttpContext.User.Identity).AddClaim(new Claim("scope", scope));
            var flowProvider = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationCodeProvider>();
            await flowProvider.OnCreateAuthorizationCodeAsync(new AuthorizationCode
            {
                Expires = properties.ExpiresUtc.Value,
                Code = code,
                Ticket = new AuthenticationTicket(context.HttpContext.User, properties, "Default")
            });

            context.HttpContext.Response.Redirect(callBackUri.ToString());
            return await result.Success(null);
        }

        private async Task<RequestResult<string>> HandleExchangeCodeAsync(RequestContext context)
        {
            var result = new RequestResult<string>(context);
            var clientProvider = context.HttpContext.RequestServices.GetRequiredService<IOauthServerProvider>();
            var client = await clientProvider.GetClientAsync(context.Require(Parameters.ClientId));
            if (client is null)
            {
                return await result.RejectAsync(Resources.Msg_InvalidClient);
            }

            if (!client.IsAllowedFlow(nameof(AuthorizationCodeGrantFlow), context))
            {
                return await result.RejectAsync(Resources.Msg_ForbiddenFlow);
            }

            var audience = context.Read(Parameters.Audience);
            if (!client.IsAllowedAudience(audience, context))
            {
                return await result.RejectAsync(Resources.Msg_InvalidAudience);
            }

            var redirectUri = context.Read(Parameters.RedirectUri);
            if (!client.IsAllowedRedirectUrl(redirectUri, context))
            {
                return await result.RejectAsync(Resources.Msg_InvalidRedirectUri);
            }

            var flowProvider = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationCodeProvider>();
            var code = await flowProvider.OnExchangeAuthorizationCodeAsync(context.Require(Parameters.Code));
            if (code is null || code.Expires <= this.Clock.UtcNow)
            {
                return await result.RejectAsync(Resources.Msg_InvalidCode);
            }

            var ticket = code.Ticket;
            Token token = new()
            {
                AccessToken = this.CreateJwtToken(context, client, string.IsNullOrWhiteSpace(audience) ? client.Audience : audience, ticket),
                AccessTokenExpiresUtc = this.Clock.UtcNow.UtcDateTime + this.Options.AccessTokenExpireTimeSpan,
                NameIdentifier = ticket.Principal.FindFirst(ClaimTypes.NameIdentifier).Value
            };

            var refreshTokenProvider = context.HttpContext.RequestServices.GetRequiredService<IRefreshTokenProvider>();
            if (refreshTokenProvider != null)
            {
                token.RefreshToken = CodeGenerator.GenerateRandomNumber(32);
                token.RefreshTokenExpiresUtc = this.Clock.UtcNow.UtcDateTime + this.Options.RefreshTokenExpireTimeSpan;
                await refreshTokenProvider.StoreAsync(token);
            }

            return await result.Success(this.CreateJwtResponse(token));
        }
    }
}