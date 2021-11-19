using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Faaast.Authentication.OAuth2Server.Core
{
    public class StageValidationContext : ValidationContext
    {
        internal ValidationContext Source { get; set; }

        public override string ClientId => this.Source.ClientId;

        public override string ClientSecret => this.Source.ClientSecret;

        public override HttpContext HttpContext => this.Source.HttpContext;

        public override string GrantType => this.Source.GrantType;

        public override string Code => this.Source.Code;

        public override string UserName => this.Source.UserName;

        public override string Password => this.Source.Password;

        public override string[] Scope { get => this.Source.Scope; set => this.Source.Scope = value; }

        public override string ResponseType => this.Source.ResponseType;

        public override string State => this.Source.State;

        public override string RedirectUri => this.Source.RedirectUri;

        public override string Audience => this.Source.Audience;

        public override Client Client { get => this.Source.Client; set => this.Source.Client = value; }

        public bool IsValidated { get; private set; }

        public bool HasError { get; private set; }

        public ErrorCodes ErrorCode { get; private set; }
        public string Error { get; private set; }

        public override string AccessToken => this.Source.AccessToken;
        public override string RefreshToken => this.Source.RefreshToken;

        public override string AppSecretProof => this.Source.AppSecretProof;

        public Task<StageValidationContext> ValidateAsync()
        {
            this.IsValidated = true;
            this.HasError = false;
            return Task.FromResult(this);
        }

        public Task<StageValidationContext> RejectAsync(ErrorCodes code, string error)
        {
            this.IsValidated = false;
            this.ErrorCode = code;
            this.HasError = !string.IsNullOrEmpty(error);
            this.Error = error;
            return Task.FromResult(this);
        }

        public StageValidationContext(ValidationContext source) => this.Source = source;
    }
}
