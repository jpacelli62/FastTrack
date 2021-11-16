using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Faaast.SeoRouter
{
    [DebuggerDisplay("{Kind}:{Url}")]
    public class RoutingRule
    {
        public virtual string DisplayName { get; set; }

        public virtual RuleKind Kind { get; set; }

        public virtual HandlerType Handler { get; set; }

        public virtual string Url { get; set; }

        public virtual MvcAction? Target { get; set; }

        public virtual bool CanGenerateUrl { get; private set; }

        public virtual bool IsDynamic { get; private set; }

        private RouteTemplate _routeTemplate;
        private TemplateMatcher _templateMatcher;
        private TemplateBinder _binder;

        private IDictionary<string, IRouteConstraint> _routeConstraints;
        private IDictionary<string, IRouteConstraint> _matchVirtualPathConstraints;


        internal RoutingRule()
        {

        }

        public RoutingRule(IServiceProvider services, string displayName, RuleKind kind, HandlerType handler, string url, MvcAction? target)
        {
            DisplayName = displayName;
            Kind = kind;
            Handler = handler;
            Url = url.NormalizeUrl();
            Target = target;

            CanGenerateUrl = Target != null && (Handler == HandlerType.Auto || Handler == HandlerType.Legacy);
            IsDynamic = url.Contains("{");
            _routeConstraints = new Dictionary<string, IRouteConstraint>();
            _matchVirtualPathConstraints = new Dictionary<string, IRouteConstraint>();
            InitConstraints(services);
        }

        internal void InitConstraints(IServiceProvider services)
        {
            if (Target == null)
                Target = new MvcAction();

            var vpdConstraints = new Dictionary<string, object>();
            var targetConstraints = Target.Value.Constraints.GetQueryDictionnary();
            var targetValues = Target.ToRouteValueDictionary();

            string template = this.Url;
            int index = template.IndexOf('?');
            if (index > -1)
            {
                var querystring = template.Substring(index + 1).GetQueryDictionnary();
                foreach (var queryParameter in querystring)
                {
                    targetConstraints.Add(queryParameter.Key, queryParameter.Value);
                    vpdConstraints.Add(queryParameter.Key, queryParameter.Value);
                }

                template = template.Substring(0, index);
            }

            _routeTemplate = TemplateParser.Parse(template);
            _templateMatcher = new TemplateMatcher(_routeTemplate, targetValues);

            var binderFactory = services.GetRequiredService<SimpleTemplateBinderFactory>();
            _binder = binderFactory.Create(_routeTemplate, _templateMatcher.Defaults);
            _routeConstraints = BuildContraints(services, targetConstraints);

            foreach (var item in targetValues)
            {
                if (!item.Key.Equals("controller", StringComparison.OrdinalIgnoreCase) &&
                   !item.Key.Equals("action", StringComparison.OrdinalIgnoreCase))
                {
                    vpdConstraints[item.Key] = item.Value;
                }
            }

            _matchVirtualPathConstraints = BuildContraints(services, vpdConstraints);
        }

        private IDictionary<string, IRouteConstraint> BuildContraints(IServiceProvider services, IDictionary<string, object> constraints)
        {
            IInlineConstraintResolver resolver = services.GetRequiredService<IInlineConstraintResolver>();
            var routeConstraintBuilder = new RouteConstraintBuilder(resolver, _routeTemplate.TemplateText);
            if (constraints != null)
            {
                foreach (var kvp in constraints)
                {
                    if (!string.IsNullOrWhiteSpace(kvp.Key) && kvp.Value != null)
                    {
                        switch (kvp.Value.ToString())
                        {
                            case "[0-9]+":
                            case "^[0-9]+$":
                                routeConstraintBuilder.AddResolvedConstraint(kvp.Key, "int");
                                break;

                            default:
                                routeConstraintBuilder.AddConstraint(kvp.Key, kvp.Value);
                                break;
                        }
                    }
                }
            }

            return routeConstraintBuilder.Build();
        }

        public virtual bool MatchStrict(string url, out RouteValueDictionary values)
        {
            values = null;
            if (Url.Equals(url))
            {
                values = Target.ToRouteValueDictionary();
                return true;
            }

            return false;
        }

        public virtual bool MatchDynamic(PathString url, out RouteValueDictionary values)
        {
            values = new RouteValueDictionary();
            return Kind == RuleKind.Global && _templateMatcher.TryMatch(url, values) && MatchConstraints(values, RouteDirection.IncomingRequest);
        }

        public virtual bool MatchConstraints(RouteValueDictionary values, RouteDirection direction)
        {
            //var constaints = direction == RouteDirection.IncomingRequest ? _routeConstraints : _matchVirtualPathConstraints;
            //if (constaints.Count > 0)
            //{
                foreach (var kvp in _matchVirtualPathConstraints)
                {
                    var constraint = kvp.Value;
                    if (!constraint.Match(null, null, kvp.Key, values, direction))
                    {
                        return false;
                    }
                }
                foreach (var kvp in _routeConstraints)
                {
                    var constraint = kvp.Value;
                    if (!constraint.Match(null, null, kvp.Key, values, direction))
                    {
                        return false;
                    }
                }
            //}

            return true;
        }

        public virtual RouteValueDictionary UrlTokens()
        {
            RouteValueDictionary values = new RouteValueDictionary();
            foreach (var parameter in _routeTemplate.Parameters)
            {
                values.Add(parameter.Name, parameter.DefaultValue);
            }

            return values;
        }


        public virtual VirtualPathData GetVirtualPath(IRouter router, RouteValueDictionary ambiantValues, RouteValueDictionary values)
        {
            if (Kind == RuleKind.Global)
            {
                foreach (var parameter in _routeTemplate.Parameters)
                {
                    if (!values.ContainsKey(parameter.Name))
                    {
                        values[parameter.Name] = parameter.Name;

                        if (ambiantValues.TryGetValue(parameter.Name, out var ambiant))
                            values[parameter.Name] = ambiant;
                    }
                }

                var binderValues = _binder.GetValues(ambiantValues, values);
                if (binderValues == null)
                {
                    // We're missing one of the required values for this route.
                    return null;
                }

                var virtualPath = _binder.BindValues(binderValues.AcceptedValues);
                if (virtualPath == null)
                {
                    return null;
                }

                VirtualPathData pathData = new VirtualPathData(router, virtualPath);
                foreach (var dataToken in binderValues.CombinedValues)
                {
                    if (!binderValues.AcceptedValues.ContainsKey(dataToken.Key))
                        pathData.DataTokens.Add(dataToken.Key, dataToken.Value);
                }

                return pathData;
            }
            else
            {
                VirtualPathData pathData = new VirtualPathData(router, Url);
                return pathData;
            }
        }
    }
}
