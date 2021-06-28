using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace FastTrack.SeoRouter
{
    public interface IMvcHandler
    {
        Task HandleMvcAsync(RouteContext context);
    }
}
