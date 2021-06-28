using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace FastTrack.SeoRouter
{
    public interface IRouteProvider
    {
        Task<RoutingRules> GetRulesAsync();

        Task<bool> RefreshNeededAsync(RoutingRules rules);

        Task ResolveUrlPartsAsync(RoutingRule rule, RouteValueDictionary ambiantValues, RouteValueDictionary values, object forRecord = null);

        Task HandleAsync(RouteContext context, RoutingRule rule);

        Task HandleRedirectAsync(RouteContext context, RoutingRule rule, VirtualPathData virtualPath);
    }
}
