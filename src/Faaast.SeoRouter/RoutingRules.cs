using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Faaast.SeoRouter
{
    public class RoutingRules
    {
        private readonly Dictionary<string, Dictionary<string, List<RoutingRule>>> _indexByControllerAction = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<RoutingRule> _allRules = new();
        private readonly List<RoutingRule> _dynamicRules = new();

        private readonly Dictionary<string, List<RoutingRule>> _indexByUrl = new();

        private readonly ReaderWriterLock _syncLock = new();

        public void Add(RoutingRule rule)
        {
            _syncLock.AcquireWriterLock(5000);
            try
            {
                _allRules.Add(rule);
                if (rule.IsDynamic || rule.Kind == RuleKind.Global)
                {
                    _dynamicRules.Add(rule);
                }

                if (!rule.IsDynamic)
                {
                    var baseUrl = rule.Url.BaseUrl();
                    if (!_indexByUrl.TryGetValue(baseUrl, out var items))
                    {
                        _indexByUrl[baseUrl] = items = new List<RoutingRule>();
                    }

                    items.Add(rule);
                }

                if (rule.CanGenerateUrl && rule.Target != null)
                {
                    _indexByControllerAction.TryAdd(rule.Target.Value.Controller, () => new Dictionary<string, List<RoutingRule>>(StringComparer.OrdinalIgnoreCase));
                    _indexByControllerAction[rule.Target.Value.Controller].TryAdd(rule.Target.Value.Action, () => new List<RoutingRule>());
                    _indexByControllerAction[rule.Target.Value.Controller][rule.Target.Value.Action].Add(rule);
                }
            }
            finally
            {
                _syncLock.ReleaseWriterLock();
            }
        }

        public void AddRange(IEnumerable<RoutingRule> rules)
        {
            if (rules != null)
            {
                foreach (var item in rules)
                {
                    this.Add(item);
                }
            }
        }

        public RoutingRule Find(string requestPath, out RouteValueDictionary values)
        {
            _syncLock.AcquireReaderLock(5000);
            try
            {
                requestPath = requestPath.NormalizeUrl();
                var baseUrl = requestPath.BaseUrl();

                if (_indexByUrl.TryGetValue(baseUrl, out var rules))
                {
                    foreach (var rule in rules)
                    {
                        if (rule.MatchStrict(requestPath, out var requestValues))
                        {
                            values = requestValues;
                            return rule;
                        }
                    }
                }

                var pathString = new PathString("/" + baseUrl);
                foreach (var rule in _dynamicRules)
                {
                    if (rule.MatchDynamic(pathString, out var requestValues))
                    {
                        if (!_indexByUrl.TryGetValue(baseUrl, out var items))
                        {
                            _indexByUrl[baseUrl] = items = new List<RoutingRule>();
                        }

                        if (!items.Contains(rule))
                        {
                            items.Add(rule);
                        }

                        values = requestValues;
                        return rule;
                    }
                }

                values = null;
                return null;
            }
            finally
            {
                _syncLock.ReleaseReaderLock();
            }
        }

        public RoutingRule FindByRoute(RouteValueDictionary values)
        {
            if (values.TryGetValue("controller", out var controller) &&
                values.TryGetValue("action", out var action) &&
                _indexByControllerAction.TryGetValue(controller.ToString(), out var actions) &&
                actions.TryGetValue(action.ToString(), out var rules))
            {
                foreach (var rule in rules)
                {
                    if (rule.MatchConstraints(values, RouteDirection.UrlGeneration))
                    {
                        return rule;
                    }
                }
            }

            return null;
        }
    }
}
