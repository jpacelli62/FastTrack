using System;
using System.Linq;
using System.Text;
using Faaast.OAuth2Server.Configuration;
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
                switch (this.HttpContext.Request.Method.ToLower())
                {
                    case "get":
                        value = this.HttpContext.Request.Query[parameter.ParameterName].FirstOrDefault();
                        break;

                    case "post":
                        value = this.HttpContext.Request.Form[parameter.ParameterName].FirstOrDefault();

                        if (string.Equals(parameter.ParameterName, Parameters.ClientId.ParameterName) || string.Equals(parameter.ParameterName, Parameters.ClientSecret.ParameterName)) // Special case for swagger
                        {
                            var basic = this.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                            if (!string.IsNullOrWhiteSpace(basic) && basic.StartsWith("Basic ", StringComparison.InvariantCultureIgnoreCase))
                            {
                                var credentials = Encoding.ASCII.GetString(Convert.FromBase64String(basic.Substring(6))).Split(':');
                                if (credentials.Length == 2)
                                {
                                    value = parameter.Equals(Parameters.ClientId) ? credentials[0] : credentials[1];
                                }
                            }
                        }

                        break;
                }
            }

            return value;
        }

        public string Require(Parameters parameter)
        {
            var value = this.Read(parameter);
            return !string.IsNullOrEmpty(value) ? value: throw new RequestException(parameter.ParameterName, string.Join(",", parameter.AllowedMethods.Select(x => x.Method).ToArray()));
        }
    }
}
