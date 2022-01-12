using System;
using System.Globalization;
using Faaast.OAuth2Server.Abstraction;
using Faaast.OAuth2Server.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Faaast.OAuth2Server
{
    public static class ApplicationBuilderExtensions
    {
        public static IServiceCollection AddFaaastOAuthServer<TProvider>(this IServiceCollection services, TProvider instance = null, Action<OAuthServerOptions> configureOptions = null)
            where TProvider : class, IOauthServerProvider
        {
            services.RegisterTypeOrInstance<IOauthServerProvider, TProvider>(instance);
            services.TryAddSingleton<ISystemClock, SystemClock>();
            if(configureOptions != null)
                services.PostConfigure<OAuthServerOptions>(configureOptions);

            return services;
        }

        public static IOauthBuilder UseFaaastOAuthServer(this IApplicationBuilder app)
        {
            var serverOptions = app.ApplicationServices.GetService<IOptions<OAuthServerOptions>>().Value;

            if (string.IsNullOrEmpty(serverOptions.Issuer))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.Issuer)));
            }

            var builder = new OauthBuilder(app, serverOptions);
            return builder;
        }

        internal static IServiceCollection RegisterTypeOrInstance<TInterface, TProvider>(this IServiceCollection services, TProvider instance = null)
            where TProvider : class, TInterface
            where TInterface : class
        {
            if (instance == null)
            {
                services.TryAddSingleton<TInterface, TProvider>();
            }
            else
            {
                services.TryAddSingleton<TInterface>(instance);
            }

            return services;

        }
    }
}
