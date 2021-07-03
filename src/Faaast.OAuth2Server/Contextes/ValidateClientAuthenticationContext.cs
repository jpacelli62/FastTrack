using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Text;

namespace Faaast.OAuth2Server
{
    public class ValidationContext
    {
        public string ClientId { get; private set; }
        public string ClientSecret { get; private set; }
        public HttpContext HttpContext { get; private set; }

        public string GrantType { get; private set; }

        public string UserName { get; private set; }
        public string Password { get; private set; }
        public string[] Scope { get; private set; }

        public string UserError { get; set; }

        public static ValidationContext Create(HttpContext context)
        {
            var requestForm = context.Request.Form;
            var clientId = requestForm[Parameters.ClientId].FirstOrDefault();
            var clientSecret = requestForm[Parameters.ClientSecret].FirstOrDefault();
            var userName = requestForm[Parameters.UserName].FirstOrDefault();
            var password = requestForm[Parameters.Password].FirstOrDefault();
            var scope = (requestForm[Parameters.Scope].FirstOrDefault() ?? string.Empty).Split(' ');

            var basic = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(basic))
            {
                if (basic.StartsWith("Basic ", StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] credentials = ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(basic.Substring(6))).Split(':');
                    if (credentials.Length == 2)
                    {
                        clientId = credentials[0];
                        clientSecret = credentials[1];
                    }
                }
            }

            return new ValidationContext
            {
                GrantType = requestForm[Parameters.GrantType].FirstOrDefault(),
                ClientId = clientId,
                ClientSecret = clientSecret,
                UserName = userName,
                Password = password,
                Scope = scope,
                HttpContext = context
            };
        }
    }
}
