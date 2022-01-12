using System;
using System.Globalization;
using Faaast.OAuth2Server.Core.Flows;
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
        
        public IOauthBuilder AddClientCredentialsGrantFlow()
        {
            if (string.IsNullOrEmpty(this.Options.TokenEndpointPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.TokenEndpointPath)));
            }

            this.Application.Map(new Microsoft.AspNetCore.Http.PathString(this.Options.TokenEndpointPath), app => app.UseMiddleware<ClientCredentialsGrantFlow>(this.Options));
            return this;
        }

        public IOauthBuilder AddResourceOwnerPasswordCredentialsGrantFlow()
        {
            if (string.IsNullOrEmpty(this.Options.TokenEndpointPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.TokenEndpointPath)));
            }

            this.Application.Map(new Microsoft.AspNetCore.Http.PathString(this.Options.TokenEndpointPath), app => app.UseMiddleware<ResourceOwnerPasswordCredentialsGrantFlow>(this.Options));
            return this;
        }

        public IOauthBuilder AddAuthorizationCodeGrantFlow()
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
                app => app.UseMiddleware<AuthorizationCodeGrantFlow>(this.Options));
            return this;
        }

        public IOauthBuilder AddRefreshTokenFlow()
        {
            if (string.IsNullOrEmpty(this.Options.TokenEndpointPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.TokenEndpointPath)));
            }

            this.Application.UseWhen(context =>
                this.Options.TokenEndpointPath.Equals(context.Request.Path, StringComparison.OrdinalIgnoreCase),
                app => app.UseMiddleware<RefreshTokenFlow>(this.Options));
            return this;
        }

        public IOauthBuilder AddImplicitGrantFlow()
        {
            if (string.IsNullOrEmpty(this.Options.AuthorizeEndpointPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.AuthorizeEndpointPath)));
            }

            this.Application.UseWhen(context =>
                this.Options.AuthorizeEndpointPath.Equals(context.Request.Path, StringComparison.OrdinalIgnoreCase),
                app => app.UseMiddleware<ImplicitGrantFlow>(this.Options));
            return this;
        }

    }
}
