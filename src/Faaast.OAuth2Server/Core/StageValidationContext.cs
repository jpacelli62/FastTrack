using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Faaast.Authentication.OAuth2Server.Core
{
    public class StageValidationContext : ValidationContext
    {
        internal ValidationContext Source { get; set; }

        public override string ClientId => Source.ClientId;

        public override string ClientSecret => Source.ClientSecret;

        public override HttpContext HttpContext => Source.HttpContext;

        public override string GrantType => Source.GrantType;

        public override string Code => Source.Code;

        public override string UserName => Source.UserName;

        public override string Password => Source.Password;

        public override string[] Scope { get => Source.Scope; set => Source.Scope = value; }


        public override string ResponseType => Source.ResponseType;

        public override string State => Source.State;

        public override string RedirectUri => Source.RedirectUri;

        public override string Audience => Source.Audience;

        public override Client Client { get => Source.Client; set => Source.Client = value; }

        public bool IsValidated { get; private set; }

        public bool HasError { get; private set; }

        public ErrorCodes ErrorCode { get; private set; }
        public string Error { get; private set; }


        public override string AccessToken => Source.AccessToken;
        public override string RefreshToken => Source.RefreshToken;

        public override string AppSecretProof => Source.AppSecretProof;

        public Task<StageValidationContext> ValidateAsync()
        {
            this.IsValidated = true;
            this.HasError = false;
            return Task.FromResult(this);
        }

        public Task<StageValidationContext> RejectAsync(ErrorCodes code, string error)
        {
            IsValidated = false;
            ErrorCode = code;
            HasError = !string.IsNullOrEmpty(error);
            Error = error;
            return Task.FromResult(this);
        }

        public StageValidationContext(ValidationContext source)
        {
            this.Source = source;
        }
    }
}
