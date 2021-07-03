using Faaast.OAuth2Server.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faaast.OAuth2Server
{
    public class OAuthServerMiddleware
    {
        private OAuthServerOptions Options { get; set; }

        private ILogger<OAuthServerMiddleware> Logger { get; set; }

        public OAuthServerMiddleware(RequestDelegate next, OAuthServerOptions options, ILoggerFactory loggerFactory)
        {
            this.Options = Options ?? throw new ArgumentNullException(nameof(Options));
            Logger = loggerFactory.CreateLogger<OAuthServerMiddleware>();
        }



        public async Task InvokeAsync(HttpContext context)
        {
            ValidationContext validation = ValidationContext.Create(context);
            PipelineStep step = new PipelineStep
            {
                Condition = (context) => Task.FromResult(!string.IsNullOrWhiteSpace(Options.TokenEndpointPath) && Options.TokenEndpointPath == context.Request.Path),
                Inner = new PipelineStep<ValidationContext>
                {
                    Input = (c) => validation,
                    Stage = "Scenario choice",
                    Condition = (context, clientContext) => Task.FromResult(Parameters.ClientCredentials.Equals(clientContext.GrantType, StringComparison.OrdinalIgnoreCase)),
                    Inner = new PipelineStep<ValidationContext>
                    {
                        Input = (c) => validation,
                        Stage = "Client credentials exact validation",
                        Condition = (context, clientContext) => ValidateClientCreadentialsAsync(context, clientContext, true),
                        Alternative = LogFailed(validation)
                    },
                    Alternative = new PipelineStep<ValidationContext>
                    {
                        Input = (c) => validation,
                        Stage = "Scenario",
                        Condition = (context, clientContext) => Task.FromResult(Parameters.Password.Equals(clientContext.GrantType, StringComparison.OrdinalIgnoreCase)),

                    }

                }



            };
        }



        private PipelineStep LogFailed(ValidationContext context)
        {
            return new PipelineStep<ValidationContext>
            {
                Input = (c) => context,
                Condition = (context, m) => Task.FromResult(true),
                Step = (c, m) =>
                {
                    Logger.LogError(context.UserError);
                    context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return context.HttpContext.Response.WriteAsync(context.UserError);
                }
            };
        }

        protected virtual Task<bool> ValidateClientCreadentialsAsync(HttpContext context, ValidationContext clientContext, bool isSecretOptional)
        {
            if (Options.ValidClients?.Any() == true)
            {
                foreach (var client in Options.ValidClients)
                {
                    if (client.ClientId.Equals(clientContext.ClientId) && client.ClientSecret.Equals(clientContext.ClientSecret))
                    {
                        return Task.FromResult(true);
                    }
                }

                clientContext.UserError = "Invalid Client";
            }
            else
            {
                clientContext.UserError = "Missing client credentials configuration";
            }

            return Task.FromResult(false);
        }
    }
}
