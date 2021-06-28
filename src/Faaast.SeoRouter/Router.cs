using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Faaast.SeoRouter
{
    public class Router : IRouter
    {
        public static readonly RoutingRule NotFound = new RoutingRule { Kind = RuleKind.Global, Handler = HandlerType.NotFound };
        public const string ControllerKey = "controller";
        public const string ActionKey = "action";

        public RoutingRules Rules { get; set; }

        public async Task<RoutingRules> GetRulesAsync(IServiceProvider services)
        {
            var provider = services.GetRequiredService<IRouteProvider>();
            if (Rules == null || await provider.RefreshNeededAsync(Rules))
            {
                Rules = await provider.GetRulesAsync();
            }

            return Rules;
        }

        public Task RouteAsync(RouteContext context)
        {
            var httpContext = context.HttpContext;
            var services = httpContext.RequestServices;
            return RouteAsync(httpContext.Request.Path.ToString(), context, services);
        }

        public Task RouteAsync(string url, RouteContext context, IServiceProvider services)
        {
            var rule = FollowRoute(url, services, out var origin, out var values);
            var provider = services.GetRequiredService<IRouteProvider>();
            if (rule != null)
            {
                if (rule == origin)
                {
                    return provider.HandleAsync(context, rule);
                }
                else
                {
                    var vpd = rule.GetVirtualPath(this, context.RouteData.DataTokens, values);
                    return provider.HandleRedirectAsync(context, rule, vpd);
                }
            }
            else//not found
            {
                return provider.HandleAsync(context, NotFound);
            }
        }

        public RoutingRule FindRouteRule(string url, IServiceProvider provider, out RouteValueDictionary routeValues)
        {
            var rules = GetRulesAsync(provider).Result;
            string requestPath = url.NormalizeUrl();
            return rules.Find(requestPath, out routeValues);
        }

        public RoutingRule FollowRoute(string url, IServiceProvider provider, out RoutingRule origin, out RouteValueDictionary values)
        {
            origin = FindRouteRule(url, provider, out values);
            if (origin != null)
            {
                switch (origin.Handler)
                {
                    case HandlerType.Auto:
                    case HandlerType.RedirectPermanent:
                    case HandlerType.RedirectTemporary:
                        return GetVirtualPathRuleAsync(values, provider).Result;

                    case HandlerType.Legacy:
                    case HandlerType.Gone:
                    case HandlerType.NotFound:
                        break;
                }
            }

            return origin;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            var httpContext = context.HttpContext;
            return GetVirtualPathAsync(context, httpContext.RequestServices).Result;
        }

        public async Task<VirtualPathData> GetVirtualPathAsync(VirtualPathContext context, IServiceProvider services, object sourceOject = null)
        {
            var rule = await GetVirtualPathRuleAsync(context.Values, services);
            if (rule != null)
            {
                var provider = services.GetRequiredService<IRouteProvider>();
                await provider.ResolveUrlPartsAsync(rule, context.AmbientValues, context.Values, sourceOject);
                return rule.GetVirtualPath(this, context.AmbientValues, context.Values);
            }

            return null;
        }

        public async Task<RoutingRule> GetVirtualPathRuleAsync(RouteValueDictionary contextValues, IServiceProvider provider)
        {
            var rules = await GetRulesAsync(provider);
            return rules.FindByRoute(contextValues);
        }
    }
}