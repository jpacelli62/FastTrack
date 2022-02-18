using System;
using System.Globalization;
using Faaast.OAuth2Server.Core.Flows;
using Microsoft.AspNetCore.Builder;

namespace Faaast.OAuth2Server.Configuration
{
    public class OauthBuilder
    {
        protected IApplicationBuilder Application { get; set; }

        protected virtual OAuthServerOptions Options { get; set; }

        public OauthBuilder(IApplicationBuilder application, OAuthServerOptions options)
        {
            this.Application = application;
            this.Options = options;
        }  
        
        public OauthBuilder AddClientCredentialsGrantFlow()
        {
            if (string.IsNullOrEmpty(this.Options.TokenEndpointPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.TokenEndpointPath)));
            }

            this.Application.UseMiddleware<ClientCredentialsGrantFlow>(this.Options);
            return this;
        }

        public OauthBuilder AddResourceOwnerPasswordCredentialsGrantFlow()
        {
            if (string.IsNullOrEmpty(this.Options.TokenEndpointPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.TokenEndpointPath)));
            }

            this.Application.UseMiddleware<ResourceOwnerPasswordCredentialsGrantFlow>(this.Options);
            return this;
        }

        public OauthBuilder AddAuthorizationCodeGrantFlow()
        {
            if (string.IsNullOrEmpty(this.Options.AuthorizeEndpointPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.AuthorizeEndpointPath)));
            }

            if (string.IsNullOrEmpty(this.Options.TokenEndpointPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.TokenEndpointPath)));
            }

            this.Application.UseMiddleware<AuthorizationCodeGrantFlow>(this.Options);
            return this;
        }

        public OauthBuilder AddRefreshTokenFlow()
        {
            if (string.IsNullOrEmpty(this.Options.TokenEndpointPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.TokenEndpointPath)));
            }

            this.Application.UseMiddleware<RefreshTokenFlow>(this.Options);
            return this;
        }

        public OauthBuilder AddImplicitGrantFlow()
        {
            if (string.IsNullOrEmpty(this.Options.AuthorizeEndpointPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.AuthorizeEndpointPath)));
            }

            this.Application.UseMiddleware<ImplicitGrantFlow>(this.Options);
            return this;
        }
        public OauthBuilder AddUserEndpoint()
        {
            if (string.IsNullOrEmpty(this.Options.UserEndpointPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.UserEndpointPath)));
            }

            this.Application.UseMiddleware<UserEndpoint>(this.Options);
            return this;
        }
        
    }
}
