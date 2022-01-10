using System;
using System.Globalization;
using Faaast.OAuth2Server.Core;
using Microsoft.AspNetCore.Builder;

namespace Faaast.OAuth2Server.Configuration
{
    public class OauthBuilder : IOauthBuilder
    {
        private IApplicationBuilder Application { get; set; }

        private OAuthServerOptions Options { get; set; }

        public OauthBuilder(IApplicationBuilder application, OAuthServerOptions options)
        {
            this.Application = application;
            this.Options = options;
        }  
        
        public IOauthBuilder AddClientCredentialsGrant()
        {
            if (string.IsNullOrEmpty(this.Options.TokenEndpointPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.TokenEndpointPath)));
            }

            this.Application.Map(new Microsoft.AspNetCore.Http.PathString(this.Options.TokenEndpointPath), app => app.UseMiddleware<ClientCredentialsGrantFlow>(this.Options));
            return this;
        }

        public IOauthBuilder AddResourceOwnerPasswordCredentialsFlow()
        {
            if (string.IsNullOrEmpty(this.Options.TokenEndpointPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.TokenEndpointPath)));
            }

            this.Application.Map(new Microsoft.AspNetCore.Http.PathString(this.Options.TokenEndpointPath), app => app.UseMiddleware<ResourceOwnerPasswordCredentialsFlow>(this.Options));
            return this;
        }

        public IOauthBuilder AddAuthorizationCodeFlow()
        {
            if (string.IsNullOrEmpty(this.Options.AuthorizeEndpointPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.AuthorizeEndpointPath)));
            }

            if (string.IsNullOrEmpty(this.Options.TokenEndpointPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.TokenEndpointPath)));
            }

            this.Application.UseWhen(context =>
                this.Options.TokenEndpointPath.Equals(context.Request.Path, StringComparison.OrdinalIgnoreCase) || this.Options.AuthorizeEndpointPath.Equals(context.Request.Path, StringComparison.OrdinalIgnoreCase), 
                app => app.UseMiddleware<AuthorizationCodeFlow>(this.Options));
            return this;
        }
    }
}
