using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Faaast.Authentication.OAuth2
{
    /// <summary>
    /// Authentication handler for FaaastOauth based authentication.
    /// </summary>
    public class FaaastOauthHandler : OAuthHandler<FaaastOauthOptions>, IAuthenticationSignOutHandler
    {
        private ILogger FaaastLog { get; set; }
        /// <summary>
        /// Initializes a new instance of <see cref="FaaastOauthHandler"/>.
        /// </summary>
        /// <inheritdoc />
        public FaaastOauthHandler(IOptionsMonitor<FaaastOauthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock) => this.FaaastLog = logger.CreateLogger("FaaastOauth");

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var result = await this.Context.AuthenticateAsync(this.SignInScheme);
            if (result != null)
            {
                if (result.Failure != null)
                {
                    return result;
                }

                // The SignInScheme may be shared with multiple providers, make sure this provider issued the identity.
                var ticket = result.Ticket;
                if (this.ShouldHandle(ticket))
                {
                    if (ticket.Properties.Items.ContainsKey(".Token.expires_at"))
                    {
                        var expire = DateTime.Parse(ticket.Properties.Items[".Token.expires_at"]);
                        return expire <= DateTime.Now ? await this.RefreshToken(result) : AuthenticateResult.Success(ticket);
                    }

                    return AuthenticateResult.Fail("Not authenticated");
                }
            }

            return AuthenticateResult.Fail("Remote authentication does not directly support AuthenticateAsync");
        }

        internal bool ShouldHandle(AuthenticationTicket ticket) => ticket != null &&
                ticket.Principal != null &&
                ticket.Properties != null &&
                ticket.Properties.Items.TryGetValue(".AuthScheme", out var authenticatedScheme) &&
                string.Equals(this.Scheme.Name, authenticatedScheme, StringComparison.Ordinal);

        internal async Task<AuthenticateResult> RefreshToken(AuthenticateResult auth)
        {
            this.FaaastLog.LogDebug("Refreshing token");
            var oAuthToken = await this.Context.CallRefreshTokenAsync(auth, this.Options);
            if (oAuthToken == null)
            {
                this.FaaastLog.LogWarning("Invalid refresh token");
                return AuthenticateResult.Fail("Invalid refresh token");
            }

            var principal = HandlerExtensions.ReadPrincipalFromToken(oAuthToken.AccessToken, this.Options);
            var newTicket = await this.CreateTicketAsync(principal.Identity as ClaimsIdentity, auth.Ticket.Properties, oAuthToken);

            await this.Context.SignOutAsync(this.SignInScheme);
            await this.Context.SignInAsync(this.SignInScheme, principal, auth.Ticket.Properties);

            return AuthenticateResult.Success(newTicket);
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            this.FaaastLog.LogDebug("Creating Oauth ticket");

            List<AuthenticationToken> authTokens = new()
            {
                new AuthenticationToken { Name = "access_token", Value = tokens.AccessToken }
            };

            if (!string.IsNullOrEmpty(tokens.RefreshToken))
            {
                authTokens.Add(new AuthenticationToken { Name = "refresh_token", Value = tokens.RefreshToken });
            }

            if (!string.IsNullOrEmpty(tokens.TokenType))
            {
                authTokens.Add(new AuthenticationToken { Name = "token_type", Value = tokens.TokenType });
            }

            if (!string.IsNullOrEmpty(tokens.ExpiresIn) && int.TryParse(tokens.ExpiresIn, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                var expiresAt = this.Clock.UtcNow + TimeSpan.FromSeconds(value);
                authTokens.Add(new AuthenticationToken
                {
                    Name = "expires_at",
                    Value = expiresAt.ToString("o", CultureInfo.InvariantCulture)
                });
            }

            properties.StoreTokens(authTokens);

            string userInfos;
            if (this.Options.UseUserInformationEndpoint)
            {
                var endpoint = QueryHelpers.AddQueryString(this.Options.UserInformationEndpoint, "access_token", tokens.AccessToken!);
                var response = await this.Backchannel.GetAsync(endpoint, this.Context.RequestAborted);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"An error occurred when retrieving user information ({response.StatusCode}). Please check if the authentication information is correct.");
                }

                userInfos = await response.Content.ReadAsStringAsync();
            }
            else
            {
                identity = HandlerExtensions.ReadPrincipalFromToken(tokens.AccessToken,this.Options).Identity as ClaimsIdentity;
                userInfos = HandlerExtensions.BuildUserPayload(identity);
            }

#if NETSTANDARD2_0 || NET461
            var payload = JObject.Parse(userInfos);
            var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, this.Context, this.Scheme, this.Options, this.Backchannel, tokens, payload);
            await this.Events.CreatingTicket(context);
            return new AuthenticationTicket(context.Principal!, context.Properties, this.Scheme.Name);
#elif NET5_0
            using var payload = JsonDocument.Parse(userInfos);
            var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, this.Context, this.Scheme, this.Options, this.Backchannel, tokens, payload.RootElement);
            context.RunClaimActions();
            await this.Events.CreatingTicket(context);
            return new AuthenticationTicket(context.Principal!, context.Properties, this.Scheme.Name);
#endif
        }

        public async Task SignOutAsync(AuthenticationProperties properties)
        {
            this.FaaastLog.LogDebug("Signin Out");

            properties ??= new AuthenticationProperties();
            var redirectUri = this.BuildRedirectUri(properties.RedirectUri ?? "/");
            var parameters = new Dictionary<string, string>
            {
                { "redirect_uri", redirectUri }
            };
            var endpoint = QueryHelpers.AddQueryString(this.Options.SignOutEndpoint, parameters);

            properties.RedirectUri = endpoint;
            await this.Context.SignOutAsync(this.Options.SignOutScheme ?? this.Options.SignInScheme, properties);
        }
    }
}
