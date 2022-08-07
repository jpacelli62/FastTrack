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
using Microsoft.AspNetCore.Http;
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

        public FaaastOauthHandler(IOptionsMonitor<FaaastOauthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock) => this.FaaastLog = logger.CreateLogger("FaaastOauth");

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var hasAccessToken = this.Context.Request.Cookies.ContainsKey(string.Concat(this.Options.CookiePrefix ?? string.Empty, "at"));
            var hasRefreshToken = this.Context.Request.Cookies.ContainsKey(string.Concat(this.Options.CookiePrefix ?? string.Empty, "rt"));

            var result = await this.Context.AuthenticateAsync(this.SignInScheme);
            if (result?.Failure != null)
            {
                return result;
            }

            var ticket = result?.Ticket;
            var authenticated = ticket?.Principal.Identity.IsAuthenticated ?? false;

            if (authenticated && hasAccessToken)
            {
                return AuthenticateResult.Success(ticket);
            }
            else if (hasRefreshToken)
            {
                return await this.RefreshToken(result);
            }
            else
            {
                return AuthenticateResult.Fail("Not authenticated");
            }
        }

        internal bool ShouldHandle(AuthenticationTicket ticket) => ticket != null &&
                ticket.Principal != null &&
                ticket.Properties != null &&
                ticket.Properties.Items.TryGetValue(".AuthScheme", out var authenticatedScheme) &&
                string.Equals(this.Scheme.Name, authenticatedScheme, StringComparison.Ordinal);

        internal async Task<AuthenticateResult> RefreshToken(AuthenticateResult auth)
        {
            this.FaaastLog.LogDebug("Refreshing token");
            var properties = auth.Ticket?.Properties ?? new AuthenticationProperties();

            if (this.Context.Request.Cookies.TryGetValue(string.Concat(this.Options.CookiePrefix ?? string.Empty, "rt"), out var token))
            {
                var json = await this.Options.Backchannel.CallRefreshTokenAsync(this.Options.TokenEndpoint, this.Options.ClientId, token);
                if (string.IsNullOrEmpty(json))
                {
                    this.FaaastLog.LogWarning("Invalid refresh token");
                    this.Context.Response.Cookies.Delete(string.Concat(this.Options.CookiePrefix ?? string.Empty, "at"));
                    this.Context.Response.Cookies.Delete(string.Concat(this.Options.CookiePrefix ?? string.Empty, "rt"));

                    return AuthenticateResult.Fail("Invalid refresh token");
                }

                var response = HandlerUtils.Parse(json);
                var principal = HandlerUtils.ReadPrincipalFromToken(response.AccessToken, this.Options);
                var newTicket = await this.CreateTicketAsync(principal.Identity as ClaimsIdentity, properties, response);

                await this.Context.SignOutAsync(this.SignInScheme);
                await this.Context.SignInAsync(this.SignInScheme, principal, properties);

                return AuthenticateResult.Success(newTicket);
            }

            return AuthenticateResult.Fail("Missing refresh cookie");
        }

        private void AddCookie(string name, string value, DateTimeOffset? expires)
        {
            var options = new CookieOptions
            {
                IsEssential = true,
                Expires = expires
            };

            var settings = this.Options.CookieOptions;
            if (settings != null)
            {
                options.Domain = settings.Domain;
                options.HttpOnly = settings.HttpOnly;
                options.MaxAge = settings.MaxAge;
                options.Path = settings.Path;
                options.SameSite = settings.SameSite;
                options.Secure = settings.Secure;
            }

            this.Context.Response.Cookies.Append(string.Concat(this.Options.CookiePrefix ?? string.Empty, name), value, options);
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            this.FaaastLog.LogDebug("Creating Oauth ticket");

            if (!string.IsNullOrEmpty(tokens.ExpiresIn) && int.TryParse(tokens.ExpiresIn, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                var expiresAt = this.Clock.UtcNow + TimeSpan.FromSeconds(value);
                this.AddCookie("at", tokens.AccessToken, expiresAt);
                this.Context.Items["at"] = tokens.AccessToken;

                if (!string.IsNullOrEmpty(tokens.RefreshToken))
                {
                    this.AddCookie("rt", tokens.RefreshToken, this.Clock.UtcNow.AddDays(30));
                    this.Context.Items["rt"] = tokens.RefreshToken;
                }
            }

            string userInfos;
            var endpoint = QueryHelpers.AddQueryString(this.Options.UserInformationEndpoint, "access_token", tokens.AccessToken!);
            endpoint = QueryHelpers.AddQueryString(endpoint, "client_id", this.Options.ClientId);
            var response = await this.Backchannel.GetAsync(endpoint, this.Context.RequestAborted);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"An error occurred when retrieving user information ({response.StatusCode}). Please check if the authentication information is correct.");
            }

            userInfos = await response.Content.ReadAsStringAsync();

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
            this.Context.Response.Cookies.Delete(string.Concat(this.Options.CookiePrefix ?? string.Empty, "at"));
            this.Context.Response.Cookies.Delete(string.Concat(this.Options.CookiePrefix ?? string.Empty, "rt"));

            properties.RedirectUri = endpoint;
            await this.Context.SignOutAsync(this.Options.SignOutScheme ?? this.Options.SignInScheme, properties);
        }
    }
}
