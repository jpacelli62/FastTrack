using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

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
        { }

        /// <inheritdoc />
        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            properties.StoreTokens(new AuthenticationToken[]
            {
                new AuthenticationToken  { Name = nameof(OAuthTokenResponse.AccessToken), Value = tokens.AccessToken },
                new AuthenticationToken  { Name = nameof(OAuthTokenResponse.RefreshToken), Value = tokens.RefreshToken }
            });

            var endpoint = QueryHelpers.AddQueryString(Options.UserInformationEndpoint, "access_token", tokens.AccessToken!);
            var response = await Backchannel.GetAsync(endpoint, Context.RequestAborted);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"An error occurred when retrieving user information ({response.StatusCode}). Please check if the authentication information is correct.");
            }

#if NETSTANDARD2_0
            var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
            var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme, Options, Backchannel, tokens, payload);
#elif NET5_0
            using (var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Context.RequestAborted)))
            {
                var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme, Options, Backchannel, tokens, payload.RootElement);
                context.RunClaimActions();
                await Events.CreatingTicket(context);
                return new AuthenticationTicket(context.Principal!, context.Properties, Scheme.Name);
            }
#endif
        }

        public async Task SignOutAsync(AuthenticationProperties properties)
        {
            properties ??= new AuthenticationProperties();
            var parameters = new Dictionary<string, string>
            {
                { "redirect_uri", BuildRedirectUri(properties.RedirectUri ?? "/") }
            };

            var endpoint = QueryHelpers.AddQueryString(Options.SignOutEndpoint, parameters);
            await Context.SignOutAsync(Options.SignOutScheme, properties);
            Response.Redirect(endpoint);
        }
    }
}
