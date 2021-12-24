﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;

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

        public RoutingRule(IServiceProvider services, string displayName, RuleKind kind, HandlerType handler, string url, MvcAction? target)
        {
            this.DisplayName = displayName;
            this.Kind = kind;
            this.Handler = handler;
            this.Url = url.NormalizeUrl();
            this.Target = target;

            this.CanGenerateUrl = this.Target != null && (this.Handler == HandlerType.Auto || this.Handler == HandlerType.Legacy);
            this.IsDynamic = url.Contains("{");
            _routeConstraints = new Dictionary<string, IRouteConstraint>();
            _matchVirtualPathConstraints = new Dictionary<string, IRouteConstraint>();
            this.InitConstraints(services);
        }

        internal void InitConstraints(IServiceProvider services)
        {
            var vpdConstraints = new Dictionary<string, object>();
            var targetConstraints = this.Target.Value.Constraints.GetQueryDictionnary();
            var targetValues = this.Target.ToRouteValueDictionary();

            var template = this.Url;
            var index = template.IndexOf('?');
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
            _routeConstraints = this.BuildContraints(services, targetConstraints);

            foreach (var item in targetValues)
            {
                if (!item.Key.Equals("controller", StringComparison.OrdinalIgnoreCase) &&
                   !item.Key.Equals("action", StringComparison.OrdinalIgnoreCase))
                {
                    vpdConstraints[item.Key] = item.Value;
                }
            }

            _matchVirtualPathConstraints = this.BuildContraints(services, vpdConstraints);
        }

        private IDictionary<string, IRouteConstraint> BuildContraints(IServiceProvider services, IDictionary<string, object> constraints)
        {
            var resolver = services.GetRequiredService<IInlineConstraintResolver>();
            var routeConstraintBuilder = new RouteConstraintBuilder(resolver, _routeTemplate.TemplateText);
            if (constraints != null)
            {
                foreach (var kvp in constraints)
                {
                    if (!string.IsNullOrWhiteSpace(kvp.Key))
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
            if (this.Url.Equals(url))
            {
                values = this.Target.ToRouteValueDictionary();
                return true;
            }

            return false;
        }

        public virtual bool MatchDynamic(PathString url, out RouteValueDictionary values)
        {
            values = new RouteValueDictionary();
            return this.Kind == RuleKind.Global && _templateMatcher.TryMatch(url, values) && this.MatchConstraints(values, RouteDirection.IncomingRequest);
        }

        public virtual bool MatchConstraints(RouteValueDictionary values, RouteDirection direction)
        {
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

            return true;
        }

        public virtual RouteValueDictionary UrlTokens()
        {
            var values = new RouteValueDictionary();
            foreach (var parameter in _routeTemplate.Parameters)
            {
                values.Add(parameter.Name, parameter.DefaultValue);
            }

            return values;
        }

        public virtual VirtualPathData GetVirtualPath(IRouter router, RouteValueDictionary ambiantValues, RouteValueDictionary values)
        {
            if (this.Kind == RuleKind.Global)
            {
                foreach (var parameter in _routeTemplate.Parameters)
                {
                    if (!values.ContainsKey(parameter.Name))
                    {
                        values[parameter.Name] = parameter.Name;

                        if (ambiantValues.TryGetValue(parameter.Name, out var ambiant))
                        {
                            values[parameter.Name] = ambiant;
                        }
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

                var pathData = new VirtualPathData(router, virtualPath);
                foreach (var dataToken in binderValues.CombinedValues)
                {
                    if (!binderValues.AcceptedValues.ContainsKey(dataToken.Key))
                    {
                        pathData.DataTokens.Add(dataToken.Key, dataToken.Value);
                    }
                }

                return pathData;
            }
            else
            {
                var pathData = new VirtualPathData(router, this.Url);
                return pathData;
            }
        }
    }
}
