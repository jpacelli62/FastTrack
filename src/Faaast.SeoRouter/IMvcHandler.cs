using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace Faaast.SeoRouter
{
    public interface IMvcHandler
    {
        Task HandleMvcAsync(RouteContext context);
    }
}
