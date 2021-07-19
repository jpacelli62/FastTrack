using Faaast.Authentication.OAuth2Server.Core;
using System.Threading.Tasks;

namespace Faaast.Authentication.OAuth2Server
{
    public interface IOauthServerProvider
    {
        Task<ClientCredential> ValidateCredentialsAsync(string clientId);

        Task<ClientCredential> ValidateCredentialsAsync(string clientId, string clientSecret);

        Task<bool> ValidateRedirectUriAsync(StageValidationContext context, ClientCredential client);

        Task<bool> RequireUserConsentAsync(StageValidationContext context, ClientCredential client);

        Task StoreAsync(Authorization code);

        Task<Authorization> RetrieveAsync(string authorizationCode);

    }
}
