using System.Security.Claims;
using System.Threading.Tasks;
using Faaast.OAuth2Server.Configuration;

namespace Faaast.OAuth2Server.Abstraction
{
    public interface IOauthServerProvider
    {
        Task<IClient> GetClientAsync(string clientId);
        //Task<ClaimsIdentity> CreateIdentityAsync(Client client);

        //Task<bool> ValidateRedirectUriAsync(RequestContext context, Client client);

        //Task<bool> RequireUserConsentAsync(RequestContext context, Client client);

        Task StoreAsync(Token token);

        Task OnCreateAuthorizationCodeAsync(AuthorizationCode code);

        Task<AuthorizationCode> OnExchangeAuthorizationCodeAsync(string code);

        //Task<Token> OnRefreshReceivedAsync(string refreshToken);

        //Task<ClaimsPrincipal> OnRefreshPrincipaldAsync(ClaimsPrincipal principal);

        //Task<ClaimsPrincipal> PasswordSigningAsync(string login, string password);
    }
}
