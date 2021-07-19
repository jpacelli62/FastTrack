using Faaast.Authentication.OAuth2Server.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;

namespace Faaast.OAuth2Server
{
    public static class ApplicationBuilderExtensions
    {
        const string xFormUrlEncoded = "application/x-www-form-urlencoded";

        public static IApplicationBuilder UseFaaastOAuthServer(this IApplicationBuilder app, Action<OAuthServerOptions> configureOptions)
        {
            var serverOptions = new OAuthServerOptions();
            configureOptions(serverOptions);
            app.MapWhen(context => IsValidRequest(context, serverOptions),
                appBuilder => appBuilder.UseMiddleware<OAuthServerMiddleware>(serverOptions));

            return app;
        }

        private static bool IsValidRequest(HttpContext context, OAuthServerOptions options)
        {
            return IsAuthorizeRequest(context, options) || IsValidTokenRequest(context, options) || IsUserRequest(context, options);
        }

        private static bool IsValidTokenRequest(HttpContext context, OAuthServerOptions options)
        {
            return context.Request.Method == HttpMethods.Post &&
                   context.Request.ContentType == xFormUrlEncoded &&
                   options.TokenEndpointPath.Equals(context.Request.Path, StringComparison.OrdinalIgnoreCase);
        }
        private static bool IsAuthorizeRequest(HttpContext context, OAuthServerOptions options)
        {
            return context.Request.Method == HttpMethods.Get &&
                   options.AuthorizeEndpointPath.Equals(context.Request.Path, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsUserRequest(HttpContext context, OAuthServerOptions options)
        {
            return context.Request.Method == HttpMethods.Get &&
                   options.UserEndpointPath.Equals(context.Request.Path, StringComparison.OrdinalIgnoreCase);
        }

    }
}
