using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace Faaast.SeoRouter
{
    public interface IRouteProvider
    {
        Task<RoutingRules> GetRulesAsync();

        Task<bool> RefreshNeededAsync(RoutingRules rules);

        Task ResolveUrlPartsAsync(RoutingRule rule, RouteValueDictionary ambiantValues, RouteValueDictionary values, object forRecord = null);

        Task HandleAsync(RouteContext context, RoutingRule rule);

        Task HandleRedirectAsync(RouteContext context, VirtualPathData virtualPath, bool permanent);
    }
}
