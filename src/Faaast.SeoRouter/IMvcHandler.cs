using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace Faaast.SeoRouter
{
    public interface IMvcHandler
    {
        Task HandleMvcAsync(RouteContext context);
    }
}
