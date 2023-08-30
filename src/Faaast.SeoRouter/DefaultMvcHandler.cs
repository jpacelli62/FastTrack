using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Faaast.SeoRouter
{
    class DefaultMvcHandler : IMvcHandler
    {
        public Task HandleMvcAsync(RouteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var services = context.HttpContext.RequestServices;
            var _actionInvokerFactory = services.GetRequiredService<IActionInvokerFactory>();
            var _actionSelector = services.GetRequiredService<IActionSelector>();

            var candidates = _actionSelector.SelectCandidates(context);
            if (candidates == null || candidates.Count == 0)
            {
                return Task.CompletedTask;
            }

            var actionDescriptor = _actionSelector.SelectBestCandidate(context, candidates);
            if (actionDescriptor == null)
            {
                return Task.CompletedTask;
            }

            context.Handler = (c) =>
            {
                var routeData = c.GetRouteData();

                var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(context.HttpContext, routeData, actionDescriptor);
                var invoker = _actionInvokerFactory.CreateInvoker(actionContext);
                return invoker == null
                    ? throw new InvalidOperationException("Could not create invoker for " + actionDescriptor.DisplayName)
                    : invoker.InvokeAsync();
            };

            return Task.CompletedTask;
        }
    }
}
