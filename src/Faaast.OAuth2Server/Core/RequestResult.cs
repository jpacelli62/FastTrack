using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Faaast.OAuth2Server.Core
{
    public class RequestResult<TResult>
    {

        public RequestContext Context { get; set; }

        public bool IsValidated { get; private set; }

        public string Error { get; private set; }

        public TResult Result { get; private set; }

        public int StatusCode { get; private set; }

        public RequestResult(RequestContext context) => this.Context = context;

        public Task<RequestResult<TResult>> RejectAsync(string error, int statusCode = StatusCodes.Status400BadRequest)
        {
            this.IsValidated = false;
            this.Error = error;
            this.StatusCode = statusCode;
            return Task.FromResult(this);
        }

        public Task<RequestResult<TResult>> Success(TResult result)
        {
            this.Result = result;
            this.IsValidated = true;
            return Task.FromResult(this);
        }
    }
}
