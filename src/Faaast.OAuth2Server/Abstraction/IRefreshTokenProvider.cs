using System.Threading.Tasks;
using Faaast.OAuth2Server.Core;

namespace Faaast.OAuth2Server.Abstraction
{
    public interface IRefreshTokenProvider
    {
        Task StoreAsync(Token token);

        Task<Token> OnRefreshTokenReceivedAsync(string refreshToken, RequestContext context);
    }
}
