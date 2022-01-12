using System.Security.Claims;
using System.Threading.Tasks;
using Faaast.OAuth2Server.Core;

namespace Faaast.OAuth2Server.Abstraction
{
    public interface IClientCredentialsProvider
    {
        Task<ClaimsIdentity> CreateClientIdentityAsync(RequestContext context, IClient client);
    }
}
