using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;

namespace FastTrack.SeoRouter
{
    public static class RouterExtensions
    {

		private const char QUERY_DELIMITER = '?';
		private const char PARAMETER_DELIMITER = '&';
		private const char PARAM_VALUE_SEPARATOR = '=';

		public static string NormalizeUrl(this string url)
        {
            url = url.ToLower();
			if (!string.IsNullOrEmpty(url) && url[0] == '/')
				url = url.Substring(1);

            return url;
        }

        public static RouteValueDictionary ToRouteValueDictionary(this MvcAction? action)
        {
            if (action == null || string.IsNullOrEmpty(action?.Controller))
                return new RouteValueDictionary();

            Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "controller", action.Value.Controller },
                { "action", action.Value.Action }
            };

            foreach (var queryPart in action.Value.DefaultValues)
            {
                values.Add(queryPart.Key, Convert.ToString(queryPart.Value));
            }

            return new RouteValueDictionary(values);
        }


        public static string BaseUrl(this string url)
        {
            int index = url.IndexOf("?");
            if (index > -1)
            {
                url = url.Substring(0, index);
            }

            return url;
        }

		public static IDictionary<string, object> GetQueryDictionnary(this string queryString)
		{
			Dictionary<string, object> query = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

			if (!string.IsNullOrWhiteSpace(queryString))
			{
				if (queryString.StartsWith(QUERY_DELIMITER.ToString()))
					queryString = queryString.Substring(1);

				int l = queryString.Length;
				int i = 0;
				while (i < l)
				{
					int si = i;
					int ti = -1;

					while (i < l)
					{
						char ch = queryString[i];

						if (ch == PARAM_VALUE_SEPARATOR)
						{
							if (ti < 0)
								ti = i;
						}
						else if (ch == PARAMETER_DELIMITER)
							break;
						i++;
					}

					// extract the name / value pair

					string name = null;
					string value = null;

					if (ti >= 0)
					{
						name = queryString.Substring(si, ti - si);
						value = queryString.Substring(ti + 1, i - ti - 1);
					}
					else
						name = queryString.Substring(si, i - si);

					if (!query.ContainsKey(name))
					{
						query.Add(name, value == null ? value : Uri.UnescapeDataString(value));
					}
					i++;
				}
			}
			return query;
		}

		public static void TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> collection, TKey key, Func<TValue> value)
        {
            if (!collection.ContainsKey(key))
            {
				collection.Add(key, value());
			}
        }
	}
}
