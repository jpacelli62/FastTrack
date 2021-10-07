using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace Faaast.Authentication.OAuth2
{
    /// <summary>
    /// Authentication handler for FaaastOauth based authentication.
    /// </summary>
    public class FaaastOauthHandler : OAuthHandler<FaaastOauthOptions>, IAuthenticationSignOutHandler
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FaaastOauthHandler"/>.
        /// </summary>
        /// <inheritdoc />
        public FaaastOauthHandler(IOptionsMonitor<FaaastOauthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var result = await Context.AuthenticateAsync(SignInScheme);
            if (result != null)
            {
                if (result.Failure != null)
                {
                    return result;
                }

                // The SignInScheme may be shared with multiple providers, make sure this provider issued the identity.
                var ticket = result.Ticket;
                if (ticket != null && ticket.Principal != null && ticket.Properties != null
                    && ticket.Properties.Items.TryGetValue(".AuthScheme", out var authenticatedScheme)
                    && string.Equals(Scheme.Name, authenticatedScheme, StringComparison.Ordinal))
                {
                    if (ticket.Properties.Items.ContainsKey(".Token.expires_at"))
                    {
                        var expire = DateTime.Parse(ticket.Properties.Items[".Token.expires_at"]);
                        if (expire <= DateTime.Now)
                        {
                            var oAuthToken = await Context.CallRefreshTokenAsync(result, Options);
                            ClaimsPrincipal principal = ReadPrincipalFromToken(oAuthToken.AccessToken);

                            var newTicket = await CreateTicketAsync(principal.Identity as ClaimsIdentity, ticket.Properties, oAuthToken);
                            await Context.SignOutAsync(SignInScheme);
                            await Context.SignInAsync(SignInScheme, principal, ticket.Properties);

                            return AuthenticateResult.Success(newTicket);
                        }

                        return AuthenticateResult.Success(ticket);
                    }

                    return AuthenticateResult.Fail("Not authenticated");
                }
            }

            return AuthenticateResult.Fail("Remote authentication does not directly support AuthenticateAsync");
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            var authTokens = new List<AuthenticationToken>();
            authTokens.Add(new AuthenticationToken { Name = "access_token", Value = tokens.AccessToken });
            if (!string.IsNullOrEmpty(tokens.RefreshToken))
            {
                authTokens.Add(new AuthenticationToken { Name = "refresh_token", Value = tokens.RefreshToken });
            }

            if (!string.IsNullOrEmpty(tokens.TokenType))
            {
                authTokens.Add(new AuthenticationToken { Name = "token_type", Value = tokens.TokenType });
            }

            if (!string.IsNullOrEmpty(tokens.ExpiresIn))
            {
                int value;
                if (int.TryParse(tokens.ExpiresIn, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                {
                    var expiresAt = Clock.UtcNow + TimeSpan.FromSeconds(value);
                    authTokens.Add(new AuthenticationToken
                    {
                        Name = "expires_at",
                        Value = expiresAt.ToString("o", CultureInfo.InvariantCulture)
                    });
                }
            }

            properties.StoreTokens(authTokens);

            string userInfos;
            if (Options.UseUserInformationEndpoint)
            {
                var endpoint = QueryHelpers.AddQueryString(Options.UserInformationEndpoint, "access_token", tokens.AccessToken!);
                var response = await Backchannel.GetAsync(endpoint, Context.RequestAborted);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"An error occurred when retrieving user information ({response.StatusCode}). Please check if the authentication information is correct.");
                }
                userInfos = await response.Content.ReadAsStringAsync();
            }
            else
            {
                identity = ReadPrincipalFromToken(tokens.AccessToken).Identity as ClaimsIdentity;
                userInfos = BuildUserPayload(identity);
            }

#if NETSTANDARD2_0 || NET461
            var payload = JObject.Parse(userInfos);
            var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme, Options, Backchannel, tokens, payload);
            await Events.CreatingTicket(context);
            return new AuthenticationTicket(context.Principal!, context.Properties, Scheme.Name);
#elif NET5_0
            using (var payload = JsonDocument.Parse(userInfos))
            {
                var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme, Options, Backchannel, tokens, payload.RootElement);
                context.RunClaimActions();
                await Events.CreatingTicket(context);
                return new AuthenticationTicket(context.Principal!, context.Properties, Scheme.Name);
            }
#endif
        }

        private string BuildUserPayload(ClaimsIdentity identity)
        {
            string[] excludeList = new[] { "iat", "exp", "iss", "nbf", "scope", "aud" };
            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream))
                {
                    writer.WriteStartObject();
                    foreach (var claim in identity.Claims)
                    {
                        bool add = true;
                        foreach (string claimToExclude in excludeList)
                        {
                            if (string.Equals(claimToExclude, claim.Type))
                            {
                                add = false;
                                break;
                            }
                        }
                        if (add)
                            writer.WriteString(claim.Type, claim.Value);
                    }
                    writer.WriteEndObject();
                }

                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private ClaimsPrincipal ReadPrincipalFromToken(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            byte[] keybytes = Encoding.ASCII.GetBytes(Options.ClientSecret);
            SecurityKey securityKey = new SymmetricSecurityKey(keybytes);
            var validationParams = new TokenValidationParameters
            {
                RequireAudience = true,
                ValidateAudience = true,
                RequireExpirationTime = true,
                ValidAudience = Options.ClientId,
                ClockSkew = TimeSpan.FromMinutes(5),
                ValidateIssuer = false,
                ValidateLifetime = true,
                RequireSignedTokens = true,
                IssuerSigningKey = securityKey,
                ValidateIssuerSigningKey = true
            };

            var principal = tokenHandler.ValidateToken(accessToken, validationParams, out var ValidatedToken);
            return principal;
        }

        public async Task SignOutAsync(AuthenticationProperties properties)
        {
            properties ??= new AuthenticationProperties();
            var parameters = new Dictionary<string, string>
            {
                { "redirect_uri", BuildRedirectUri(properties.RedirectUri ?? "/") }
            };

            var schemeProvider = Context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
            var endpoint = QueryHelpers.AddQueryString(Options.SignOutEndpoint, parameters);
            await Context.SignOutAsync((await schemeProvider.GetDefaultSignOutSchemeAsync()).Name, properties);
            Response.Redirect(endpoint);
        }
    }
}
