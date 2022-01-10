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
    public class AuthorizationCodeFlow : OAuthMiddleware
    {
        public AuthorizationCodeFlow(RequestDelegate next, OAuthServerOptions options, ILoggerFactory loggerFactory, ISystemClock clock) : base(next, options, loggerFactory, clock)
        {
        }

        protected override bool ShouldHandle(RequestContext context) => IsChallenge(context) || IsExchangeCode(context);

        private static bool IsChallenge(RequestContext context) => HttpMethods.IsGet(context.HttpContext.Request.Method) && string.Equals(Parameters.Code.ParameterName, context.Read(Parameters.ResponseType));

        private static bool IsExchangeCode(RequestContext context) => HttpMethods.IsPost(context.HttpContext.Request.Method) && string.Equals(Parameters.AuthorizationCode, context.Read(Parameters.GrantType));

        protected override async Task<RequestResult<string>> HandleAsync(RequestContext context, IOauthServerProvider provider) => IsChallenge(context) ? await this.HandleChallengeAsync(context, provider) : await this.HandleExchangeCodeAsync(context, provider);

        private async Task<RequestResult<string>> HandleChallengeAsync(RequestContext context, IOauthServerProvider provider)
        {
            var result = new RequestResult<string>(context);
            var client = await provider.GetClientAsync(context.Require(Parameters.ClientId));
            if (client is null)
            {
                return await result.RejectAsync(Resources.Msg_InvalidClient);
            }

            if (!client.IsAllowedFlow(nameof(AuthorizationCodeFlow), context))
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
                return !string.IsNullOrEmpty(this.Options.UserConsentPath)
                    ? await this.RedirectAsync(context, this.Options.UserConsentPath)
                    : await result.RejectAsync(Resources.Msg_InvalidScope);
            }

            var code = CodeGenerator.GenerateRandomNumber(32);
            var uri = new Uri(redirectUri);
            var query = string.Format("?code={0}&state={1}", code, context.Read(Parameters.State));
            var callBackUri = new UriBuilder(uri.Scheme, uri.Host, uri.Port, uri.LocalPath, query);

            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUri,
                IssuedUtc = this.Clock.UtcNow,
                ExpiresUtc = this.Clock.UtcNow + TimeSpan.FromMinutes(5)
            };

            properties.SetString("Scope", scope);
            await provider.OnCreateAuthorizationCodeAsync(new AuthorizationCode
            {
                Expires = properties.ExpiresUtc.Value,
                Code = code,
                Ticket = new AuthenticationTicket(context.HttpContext.User, properties, "Default")
            });

            context.HttpContext.Response.Redirect(callBackUri.ToString());
            return await result.Success(null);
        }

        private async Task<RequestResult<string>> HandleExchangeCodeAsync(RequestContext context, IOauthServerProvider provider)
        {
            var result = new RequestResult<string>(context);
            var client = await provider.GetClientAsync(context.Require(Parameters.ClientId));
            if (client is null || !client.ClientSecret.Equals(context.Require(Parameters.ClientSecret)))
            {
                return await result.RejectAsync(Resources.Msg_InvalidClient);
            }

            if (!client.IsAllowedFlow(nameof(AuthorizationCodeFlow), context))
            {
                return await result.RejectAsync(Resources.Msg_ForbiddenFlow);
            }

            var redirectUri = context.Read(Parameters.RedirectUri);
            if (!client.IsAllowedRedirectUrl(redirectUri, context))
            {
                return await result.RejectAsync(Resources.Msg_InvalidRedirectUri);
            }

            var code = await provider.OnExchangeAuthorizationCodeAsync(context.Require(Parameters.Code));
            if (code is null || code.Expires <= this.Clock.UtcNow)
            {
                return await result.RejectAsync(Resources.Msg_InvalidCode);
            }

            var ticket = code.Ticket;
            Token token = new()
            {
                AccessToken = this.CreateJwtToken(context, client, null, ticket),
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