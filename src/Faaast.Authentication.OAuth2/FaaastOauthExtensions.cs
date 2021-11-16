using Faaast.Authentication.OAuth2;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Net.Http;
using System.Threading;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to configure OAuth authentication.
    /// </summary>
    public static class FaaastOauthAuthenticationOptionsExtensions
    {
        /// <summary>
        /// Adds OAuth-based authentication to <see cref="AuthenticationBuilder"/> using the default scheme.
        /// The default scheme is specified by <see cref="FaaastOauthDefaults.AuthenticationScheme"/>.
        /// <para>
        /// FaaastOauth authentication allows application users to sign in to their FaaastOauth SSO.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
        /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
        public static AuthenticationBuilder AddFaaastOauth(this AuthenticationBuilder builder)
            => builder.AddFaaastOauth(FaaastOauthDefaults.AuthenticationScheme, _ => { });

        /// <summary>
        /// Adds OAuth-based authentication to <see cref="AuthenticationBuilder"/> using the default scheme.
        /// The default scheme is specified by <see cref="FaaastOauthDefaults.AuthenticationScheme"/>.
        /// <para>
        /// FaaastOauth authentication allows application users to sign in to their FaaastOauth SSO.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
        /// <param name="configureOptions">A delegate to configure <see cref="FaaastOauthOptions"/>.</param>
        /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
        public static AuthenticationBuilder AddFaaastOauth(this AuthenticationBuilder builder, Action<FaaastOauthOptions> configureOptions)
            => builder.AddFaaastOauth(FaaastOauthDefaults.AuthenticationScheme, configureOptions);

        /// <summary>
        /// Adds OAuth-based authentication to <see cref="AuthenticationBuilder"/> using the default scheme.
        /// The default scheme is specified by <see cref="FaaastOauthDefaults.AuthenticationScheme"/>.
        /// <para>
        /// FaaastOauth authentication allows application users to sign in to their FaaastOauth SSO.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
        /// <param name="authenticationScheme">The authentication scheme.</param>
        /// <param name="configureOptions">A delegate to configure <see cref="FaaastOauthOptions"/>.</param>
        /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
        public static AuthenticationBuilder AddFaaastOauth(this AuthenticationBuilder builder, string authenticationScheme, Action<FaaastOauthOptions> configureOptions)
            => builder.AddFaaastOauth(authenticationScheme, FaaastOauthDefaults.DisplayName, configureOptions);

        /// <summary>
        /// Adds OAuth-based authentication to <see cref="AuthenticationBuilder"/> using the default scheme.
        /// The default scheme is specified by <see cref="FaaastOauthDefaults.AuthenticationScheme"/>.
        /// <para>
        /// FaaastOauth authentication allows application users to sign in to their FaaastOauth SSO.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
        /// <param name="authenticationScheme">The authentication scheme.</param>
        /// <param name="displayName">A display name for the authentication handler.</param>
        /// <param name="configureOptions">A delegate to configure <see cref="FaaastOauthOptions"/>.</param>
        public static AuthenticationBuilder AddFaaastOauth(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<FaaastOauthOptions> configureOptions)
            => builder.AddOAuth<FaaastOauthOptions, FaaastOauthHandler>(authenticationScheme, displayName, configureOptions);
    }
}
