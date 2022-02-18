using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Faaast.OAuth2Server.Abstraction;
using Faaast.OAuth2Server.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Faaast.OAuth2Server.Core.Flows
{
    public class RefreshTokenFlow : OAuthMiddleware
    {
        public RefreshTokenFlow(RequestDelegate next, OAuthServerOptions options, ILoggerFactory loggerFactory, ISystemClock clock) : base(next, options, loggerFactory, clock)
        {
        }

        protected override bool MatchEndpoint(RequestContext context) => this.Options.TokenEndpointPath.Equals(context.HttpContext.Request.Path, StringComparison.OrdinalIgnoreCase);

        protected override bool ShouldHandle(RequestContext context) => HttpMethods.IsPost(context.HttpContext.Request.Method) && string.Equals(Parameters.RefreshToken.ParameterName, context.Read(Parameters.GrantType));

        protected override async Task<RequestResult<string>> HandleAsync(RequestContext context)
        {
            var result = new RequestResult<string>(context);
            var clientId = context.Require(Parameters.ClientId);
            var clientProvider = context.HttpContext.RequestServices.GetRequiredService<IOauthServerProvider>();
            var client = await clientProvider.GetClientAsync(clientId);
            if(client != null && client.IsAllowedFlow(nameof(RefreshTokenFlow), context))
            {
                var refreshProvider = context.HttpContext.RequestServices.GetRequiredService<IRefreshTokenProvider>();
                var token = await refreshProvider.OnRefreshTokenReceivedAsync(context.Require(Parameters.RefreshToken), context);
                if (token is null || token.RefreshTokenExpiresUtc <= this.Clock.UtcNow)
                {
                    return await result.RejectAsync(Resources.Msg_InvalidToken);
                }

                var validationParams = this.BuildValidationParameters(client, context);
                var tokenHandler = new JwtSecurityTokenHandler();
                ClaimsPrincipal principal = null;
                try
                {
                    principal = tokenHandler.ValidateToken(token.AccessToken, validationParams, out _);
                    var identity = principal.Identity as ClaimsIdentity;
                    identity.TryRemoveClaim(identity.FindFirst("nbf"));
                    identity.TryRemoveClaim(identity.FindFirst("exp"));
                    identity.TryRemoveClaim(identity.FindFirst("iss"));
                    identity.TryRemoveClaim(identity.FindFirst("iat"));
                    identity.TryRemoveClaim(identity.FindFirst("aud"));
                }
                catch
                {
                    return await result.RejectAsync(Resources.Msg_InvalidToken);
                }
                
                var jwt = tokenHandler.ReadJwtToken(token.AccessToken);
                if (jwt.Audiences is not null && !jwt.Audiences.All(x => client.IsAllowedAudience(x, context)))
                {
                    return await result.RejectAsync(Resources.Msg_InvalidAudience);
                }

                var scope = principal.FindFirst("scope");
                if (!string.IsNullOrWhiteSpace(scope?.Value) && !client.IsAllowedScope(scope.Value, principal.Identity as ClaimsIdentity, context))
                {
                    return await result.RejectAsync(Resources.Msg_InvalidScope);
                }

                var ticket = refreshProvider.CreateTicket(principal.Identity, context);
                if(ticket == null)
                {
                    this.Logger.LogDebug("Rejected by app");
                    return await result.RejectAsync(Resources.Msg_InvalidToken);
                }

                var descriptor = new SecurityTokenDescriptor()
                {
                    Audience = string.Join(" ", jwt.Audiences?.ToArray() ?? Array.Empty<string>()),
                    Issuer = this.Options.Issuer,
                    SigningCredentials = client.GetSigninCredentials(context),
                    IssuedAt = this.Clock.UtcNow.UtcDateTime,
                    Expires = this.Clock.UtcNow.UtcDateTime + this.Options.AccessTokenExpireTimeSpan,
                    Subject = ticket.Principal.Identity as ClaimsIdentity
                };

                token.AccessToken = tokenHandler.CreateEncodedJwt(descriptor);
                token.AccessTokenExpiresUtc = descriptor.Expires.Value;
                token.RefreshToken = CodeGenerator.GenerateRandomNumber(32);
                token.RefreshTokenExpiresUtc = this.Clock.UtcNow.UtcDateTime + this.Options.RefreshTokenExpireTimeSpan;
                await refreshProvider.StoreAsync(token);

                return await result.Success(this.CreateJwtResponse(token));
            }

            return await result.RejectAsync(Resources.Msg_InvalidClient);
        }
    }
}
