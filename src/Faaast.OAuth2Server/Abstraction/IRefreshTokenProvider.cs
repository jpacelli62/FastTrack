using System.Security.Principal;
using System.Threading.Tasks;
using Faaast.OAuth2Server.Core;
using Microsoft.AspNetCore.Authentication;

namespace Faaast.OAuth2Server.Abstraction
{
    public interface IRefreshTokenProvider
    {
        Task StoreAsync(Token token);

        Task<Token> OnRefreshTokenReceivedAsync(string refreshToken, RequestContext context);

        AuthenticationTicket CreateTicket(IIdentity identity, RequestContext context);
    }
}
