using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

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
            IActionInvokerFactory _actionInvokerFactory = services.GetRequiredService<IActionInvokerFactory>();
            IActionSelector _actionSelector = services.GetRequiredService<IActionSelector>();

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

                var actionContext = new ActionContext(context.HttpContext, routeData, actionDescriptor);
                var invoker = _actionInvokerFactory.CreateInvoker(actionContext);
                if (invoker == null)
                {
                    throw new InvalidOperationException("Could not create invoker for " + actionDescriptor.DisplayName);
                }

                return invoker.InvokeAsync();
            };

            return Task.CompletedTask;
        }
    }
}
