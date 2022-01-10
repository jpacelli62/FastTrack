using System;
using System.Globalization;
using Faaast.OAuth2Server.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Faaast.OAuth2Server
{
    public static class ApplicationBuilderExtensions
    {
        public static IServiceCollection AddFaaastOAuthServer(this IServiceCollection services)
        {
            services.TryAddSingleton<ISystemClock, SystemClock>();
            return services;
        }

        public static IOauthBuilder UseFaaastOAuthServer(this IApplicationBuilder app, Action<OAuthServerOptions> configureOptions)
        {
            var serverOptions = new OAuthServerOptions();
            configureOptions(serverOptions);
            if (string.IsNullOrEmpty(serverOptions.Issuer))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.Issuer)));
            }

            var builder = new OauthBuilder(app, serverOptions);
            return builder;
        }
    }
}
