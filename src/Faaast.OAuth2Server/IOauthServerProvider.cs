using System.Security.Claims;
using System.Threading.Tasks;
using Faaast.Authentication.OAuth2Server.Core;

namespace Faaast.Authentication.OAuth2Server
{
    public interface IOauthServerProvider
    {
        Task<ClientCredential> ValidateCredentialsAsync(string clientId);

        Task<ClientCredential> ValidateCredentialsAsync(string clientId, string clientSecret);

        Task<bool> ValidateRedirectUriAsync(StageValidationContext context, ClientCredential client);

        Task<bool> RequireUserConsentAsync(StageValidationContext context, ClientCredential client);

        Task OnCreateAuthorizationCode(Authorization code);

        Task StoreAsync(Token token);

        Task<Authorization> OnExchangeAuthorizationCode(string authorizationCode);

        Task<Token> OnRefreshReceivedAsync(string refreshToken);

        Task<ClaimsPrincipal> OnRefreshPrincipaldAsync(ClaimsPrincipal principal);

        Task<ClaimsPrincipal> PasswordSigningAsync(string login, string password);
    }
}
