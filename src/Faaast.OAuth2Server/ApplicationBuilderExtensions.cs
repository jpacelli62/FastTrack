using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;

namespace Faaast.OAuth2Server
{
    public static class ApplicationBuilderExtensions
    {
        const string xFormUrlEncoded = "application/x-www-form-urlencoded";

        public static IApplicationBuilder UseOAuthServer(this IApplicationBuilder app, Action<OAuthServerOptions> configureOptions)
        {
            var jwtServerOptions = new OAuthServerOptions();
            configureOptions(jwtServerOptions);
            app.MapWhen(context => IsValidRequest(context, jwtServerOptions.TokenEndpointPath),
                      appBuilder => appBuilder.UseMiddleware<OAuthServerOptions>(jwtServerOptions));

            return app;
        }

        private static bool IsValidRequest(HttpContext context, string tokenPath)
        {
            return context.Request.Method == HttpMethods.Post &&
                   context.Request.ContentType == xFormUrlEncoded &&
                   context.Request.Path == tokenPath;
        }
    }
}
