using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Faaast.OAuth2Server.Core
{
    public class RequestContext
    {
        public virtual HttpContext HttpContext { get; private set; }

        public RequestContext(HttpContext httpContext) => this.HttpContext = httpContext;

        public string Read(Parameters parameter)
        {
            string value = null;
            if (parameter.AllowedMethods.Any(x => this.HttpContext.Request.Method.Equals(x.Method, StringComparison.OrdinalIgnoreCase)))
            {
                value = this.HttpContext.Request.Method.ToLower() switch
                {
                    "get" => ReadGet(this.HttpContext.Request, parameter),
                    "post" => ReadPost(this.HttpContext.Request, parameter),
                    _ => value
                };
            }

            return value;
        }

        private static string ReadGet(HttpRequest request, Parameters parameter) => request.Query[parameter.ParameterName].FirstOrDefault();

        private static string ReadPost(HttpRequest request, Parameters parameter) => request.Form[parameter.ParameterName].FirstOrDefault();

        public string Require(Parameters parameter)
        {
            var value = this.Read(parameter);
            return !string.IsNullOrEmpty(value) ? value: throw new RequestException(parameter.ParameterName, string.Join(",", parameter.AllowedMethods.Select(x => x.Method).ToArray()));
        }
    }
}
