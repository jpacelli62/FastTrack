using System;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace Faaast.SeoRouter
{
    internal class SeoRouteBuilder : ISeoRouteBuilder
    {
        private RoutingRules StaticRules { get; set; }
        private IServiceProvider Provider { get; set; }

        public SeoRouteBuilder(IServiceProvider provider)
        {
            this.Provider = provider;
            this.StaticRules = new RoutingRules();
        }

        public ISeoRouteBuilder MapRoute(string name, string template, object defaults = null)
        {
            if (defaults != null)
            {
                var values = new RouteValueDictionary(defaults);
                var controller = values["controller"].ToString();
                var action = values["action"].ToString();
                values.Remove("controller");
                values.Remove("action");
                var additionalValues =
                    string.Join("&", values.Select(x => string.Concat(x.Key, "=", x.Value)).ToArray());
                this.StaticRules.Add(new RoutingRule(this.Provider, name, RuleKind.Global, HandlerType.Auto, template,
                    new MvcAction(controller, action, additionalValues)));
            }else if (template.Contains("{"))
            {
                var parsedTemplate = TemplateParser.Parse(template);
                var controller = parsedTemplate.Parameters.First(x=> string.Equals(x.Name, "controller", StringComparison.OrdinalIgnoreCase)).Name;
                var action = parsedTemplate.Parameters.First(x=> string.Equals(x.Name, "action", StringComparison.OrdinalIgnoreCase)).Name;
                this.StaticRules.Add(new RoutingRule(this.Provider, name, RuleKind.Global, HandlerType.Auto, template,
                    new MvcAction(controller, action, string.Empty)));

            }

            return this;
        }

        public RoutingRules Build() => this.StaticRules;
    }
}
