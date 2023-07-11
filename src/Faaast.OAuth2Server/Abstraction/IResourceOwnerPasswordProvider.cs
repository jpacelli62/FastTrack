using System.Security.Claims;
using System.Threading.Tasks;
using Faaast.OAuth2Server.Core;

namespace Faaast.OAuth2Server.Abstraction
{
    public interface IResourceOwnerPasswordProvider
    {
        Task<RequestResult<ClaimsIdentity>> PasswordSignInAsync(string username, string password, IClient client, RequestContext context);
    }
}
