using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Text;

namespace Faaast.Authentication.OAuth2Server.Core
{
    public class ValidationContext
    {
        public virtual string ClientId { get; private set; }

        public virtual string ClientSecret { get; private set; }

        public virtual HttpContext HttpContext { get; private set; }

        public virtual string GrantType { get; private set; }

        public virtual string Code { get; private set; }

        public virtual string UserName { get; private set; }

        public virtual string Password { get; private set; }

        public virtual string[] Scope { get; set; }

        public virtual string Audience { get; set; }


        public virtual string ResponseType { get; private set; }

        public virtual string State { get; private set; }

        public virtual string RedirectUri { get; private set; }

        public virtual string AccessToken { get; private set; }
        public virtual string RefreshToken { get; private set; }
        public virtual string AppSecretProof { get; private set; }




        public virtual ClientCredential Client { get; set; }

        public static ValidationContext Create(HttpContext context)
        {
            if (HttpMethods.IsPost(context.Request.Method))
            {
                var requestForm = context.Request.Form;
                var validationContext = new ValidationContext
                {
                    GrantType = requestForm[Parameters.GrantType].FirstOrDefault(),
                    ClientId = requestForm[Parameters.ClientId].FirstOrDefault(),
                    ClientSecret = requestForm[Parameters.ClientSecret].FirstOrDefault(),
                    UserName = requestForm[Parameters.UserName].FirstOrDefault(),
                    Password = requestForm[Parameters.Password].FirstOrDefault(),
                    Code = requestForm[Parameters.Code].FirstOrDefault(),
                    Scope = (requestForm[Parameters.Scope].FirstOrDefault()?.Split(' ') ?? new string[0]),
                    RedirectUri = requestForm[Parameters.RedirectUri].FirstOrDefault(),
                    RefreshToken = requestForm[Parameters.RefreshToken].FirstOrDefault(),
                    AccessToken = requestForm[Parameters.AccessToken].FirstOrDefault(),
                    HttpContext = context
                };

                var basic = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(basic) && basic.StartsWith("Basic ", StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] credentials = ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(basic.Substring(6))).Split(':');
                    if (credentials.Length == 2)
                    {
                        validationContext.ClientId = credentials[0];
                        validationContext.ClientSecret = credentials[1];
                    }
                }

                return validationContext;
            }
            else if (HttpMethods.IsGet(context.Request.Method))
            {
                var query = context.Request.Query;
                return new ValidationContext
                {
                    ClientId = query[Parameters.ClientId].FirstOrDefault(),
                    Scope = (query[Parameters.Scope].FirstOrDefault() ?? string.Empty).Split(' '),
                    ResponseType = query[Parameters.ResponseType].FirstOrDefault() ?? string.Empty,
                    State = query[Parameters.State].FirstOrDefault() ?? string.Empty,
                    RedirectUri = query[Parameters.RedirectUri].FirstOrDefault(),
                    HttpContext = context,
                    AppSecretProof = query[Parameters.AppSecretProof].FirstOrDefault(),
                    AccessToken = query[Parameters.AccessToken].FirstOrDefault(),
                };
            }
            throw new NotImplementedException();
        }
    }
}
