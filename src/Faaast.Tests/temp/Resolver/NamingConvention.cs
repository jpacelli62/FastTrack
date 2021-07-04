using System;
using System.Collections.Generic;
using System.Linq;

namespace Faaast.Orm.Resolver
{
    public class NamingConvention
    {
        private List<string> _dbPrefixToRemove;
        private string _modelPrefix;
        private string _modelSuffix;
        private Dictionary<string, string> _replace;
        private List<string> _changeCaseOn;
        private List<Func<string, string>> _filters;
        private List<Func<string, bool>> _predicates;

        public NamingConvention()
        {
            _filters = new List<Func<string, string>>();
            _predicates = new List<Func<string, bool>>();
            _dbPrefixToRemove = new List<string> { "tbl_", "joi_" };
            _replace = new Dictionary<string, string>
            {
                { "@@", "@" }
            };
            _changeCaseOn = new List<string>() { "_", " ", ".", "@", "-" };
        }

        public virtual string Format(string name)
        {
            if (_predicates.Any(x => !x(name)))
                return null;

            foreach (var filter in _filters)
            {
                name = filter(name);
            }

            foreach (var prefix in _dbPrefixToRemove)
            {
                if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    name = name.Substring(prefix.Length);
            }

            foreach (var replace in _replace)
            {
                name = name.Replace(replace.Key, replace.Value);
            }

            List<string> newName = new List<string>() { _modelPrefix };
            newName.AddRange(name.Split(_changeCaseOn.ToArray(), StringSplitOptions.RemoveEmptyEntries).Select(FormatWord));
            newName.Add(_modelSuffix);
            return string.Concat(newName.ToArray());
        }

        internal virtual string FormatWord(string part)
        {
            if (string.IsNullOrWhiteSpace(part))
                return string.Empty;

            return string.Concat
            (
                part.ToUpper()[0],
                part.Length == 1 ? string.Empty : part.ToLower().Substring(1)
            );
        }

        public NamingConvention RemoveTablePrefixes(params string[] prefixes)
        {
            this._dbPrefixToRemove.AddRange(prefixes);
            return this;
        }

        public NamingConvention AddPrefixToName(string prefix)
        {
            this._modelPrefix = prefix;
            return this;
        }

        public NamingConvention AddSuffixToName(string suffix)
        {
            this._modelSuffix = suffix;
            return this;
        }

        public NamingConvention ReplaceInName(string search, string replacemnt)
        {
            this._replace.Add(search, replacemnt);
            return this;
        }

        public NamingConvention Filter(Func<string, string> filter)
        {
            this._filters.Add(filter);
            return this;
        }

        public NamingConvention Match(Func<string, bool> predicate)
        {
            this._predicates.Add(predicate);
            return this;
        }
    }
}
