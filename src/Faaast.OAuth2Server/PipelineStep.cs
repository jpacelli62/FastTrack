using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Faaast.OAuth2Server
{

    public class PipelineStep
    {
        public string Stage { get; set; }

        public PipelineStep Inner { get; set; }
        public PipelineStep Alternative { get; set; }


        public Func<HttpContext, Task<bool>> Condition { get; set; } = (c) => Task.FromResult(false);

        public virtual async Task ExecuteAsync(HttpContext context, ILogger log)
        {
            if (await Condition(context))
            {
                Inner?.ExecuteAsync(context, log);
            }
            else
            {
                Alternative?.ExecuteAsync(context, log);
            }
        }
    }


    public class PipelineStep<TInput> : PipelineStep
    {
        public Func<HttpContext, TInput> Input { get; set; }

        public Func<HttpContext, TInput, Task> Step { get; set; } = (c,i) => Task.CompletedTask;

        public new Func<HttpContext, TInput, Task<bool>> Condition { get; set; } = (c, i) => Task.FromResult(false);

        public async Task ExecuteAsync(HttpContext context, TInput input, ILogger log)
        {
            if (await Condition(context, input))
            {
                await Step(context, input);
                Inner?.ExecuteAsync(context, log);
            }
            else
            {
                Alternative?.ExecuteAsync(context, log);
            }
        }

        public override Task ExecuteAsync(HttpContext context, ILogger log)
        {
            return ExecuteAsync(context, Input(context), log);
        }
    }
}
