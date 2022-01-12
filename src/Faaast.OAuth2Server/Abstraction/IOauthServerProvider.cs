using System.Threading.Tasks;

namespace Faaast.OAuth2Server.Abstraction
{
    public interface IOauthServerProvider
    {
        Task<IClient> GetClientAsync(string clientId);
    }
}
