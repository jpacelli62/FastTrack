using System.Threading.Tasks;

namespace Faaast.OAuth2Server.Abstraction
{
    public interface IAuthorizationCodeProvider
    {
        Task OnCreateAuthorizationCodeAsync(AuthorizationCode code);

        Task<AuthorizationCode> OnExchangeAuthorizationCodeAsync(string code);
    }
}
