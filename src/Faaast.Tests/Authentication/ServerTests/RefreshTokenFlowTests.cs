using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Faaast.OAuth2Server.Core;
using Faaast.Tests.Authentication.Utility;
using Microsoft.AspNetCore.TestHost;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Faaast.Tests.Authentication.ServerTests
{
    public class RefreshTokenFlowTests : IClassFixture<ServerFixture>
    {
        public ServerFixture Fixture { get; set; }

        public CustomTestServer Server { get; set; }

        public RefreshTokenFlowTests(ServerFixture fixture)
        {
            this.Fixture = fixture;
            this.Server = fixture.CreateServer(builder => builder.AddRefreshTokenFlow());
            CreateToken(this.Fixture);
        }

        private async Task<Transaction> QueryAsync(CustomTestServer server, string clientId, string refreshToken, Action<HttpRequestMessage> req = null) =>
            await server.SendPostAsync(this.Fixture.TokenEndpoint, new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "client_id", clientId},
                { "refresh_token", refreshToken }
            }, req);

        private static void CreateToken(ServerFixture fixture, Action<SecurityTokenDescriptor> jwt = null)
        {
            List<Claim> claims = new();
            claims.Add(new Claim(ClaimTypes.Name, "John Doe"));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, "123"));
            var identity = new ClaimsIdentity(claims);

            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var utcNow = fixture.Clock.UtcNow;
            var expires = utcNow + fixture.Options.AccessTokenExpireTimeSpan;
            var descriptor = new SecurityTokenDescriptor()
            {
                Audience = fixture.Client.Audience,
                Issuer = fixture.Options.Issuer,
                SigningCredentials = fixture.Client.GetSigninCredentials(null),
                IssuedAt = utcNow.UtcDateTime,
                Expires = expires.UtcDateTime,
                Subject = identity,
                Claims = new Dictionary<string, object>()
            };
            jwt?.Invoke(descriptor);
            var jwtToken = tokenHandler.CreateJwtSecurityToken(descriptor);

            var accessToken = tokenHandler.WriteToken(jwtToken);
            fixture.Token = new OAuth2Server.Abstraction.Token()
            {
                AccessToken = accessToken,
                AccessTokenExpiresUtc = fixture.Clock.UtcNow.UtcDateTime + fixture.Options.AccessTokenExpireTimeSpan,
                NameIdentifier = "123",
                RefreshToken = CodeGenerator.GenerateRandomNumber(32),
                RefreshTokenExpiresUtc = fixture.Clock.UtcNow.UtcDateTime + fixture.Options.RefreshTokenExpireTimeSpan
            };
        }

        [Fact]
        public async Task Test_should_not_handle()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, this.Fixture.TokenEndpoint);
            var transaction = await this.Server.SendAsync(request);
            Assert.Equal(HttpStatusCode.NotFound, transaction.Response.StatusCode);
        }

        [Fact]
        public async Task Test_invalid_client()
        {
            var transaction = await this.QueryAsync(this.Server, "wrongid", this.Fixture.Token?.RefreshToken);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidClient, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_invalid_token()
        {
            var transaction = await this.QueryAsync(this.Server, this.Fixture.Client.ClientId, "badToken");
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidToken, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_expired_token()
        {
            var fixture = new ServerFixture();
            var server = fixture.CreateServer(builder => builder.AddRefreshTokenFlow());
            CreateToken(fixture);
            Assert.NotNull(fixture.Token);
            fixture.Token.RefreshTokenExpiresUtc = fixture.Clock.UtcNow.AddHours(-5).UtcDateTime;

            var transaction = await this.QueryAsync(server, fixture.Client.ClientId, fixture.Token.RefreshToken);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidToken, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_invalid_signinCredentials()
        {
            var fixture = new ServerFixture();
            var server = fixture.CreateServer(builder => builder.AddRefreshTokenFlow());
            CreateToken(fixture, jwt => jwt.SigningCredentials = new(new SymmetricSecurityKey(Encoding.Default.GetBytes("wron6df4g6df54g65df4ggsecuritykey")), SecurityAlgorithms.HmacSha256));
            Assert.NotNull(fixture.Token);

            var transaction = await this.QueryAsync(server, fixture.Client.ClientId, fixture.Token.RefreshToken);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidToken, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_invalid_Issuer()
        {
            var fixture = new ServerFixture();
            var server = fixture.CreateServer(builder => builder.AddRefreshTokenFlow());
            CreateToken(fixture, jwt => jwt.Issuer = "Not me");
            Assert.NotNull(fixture.Token);

            var transaction = await this.QueryAsync(server, fixture.Client.ClientId, fixture.Token.RefreshToken);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidToken, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_invalid_audience()
        {
            var fixture = new ServerFixture();
            var server = fixture.CreateServer(builder => builder.AddRefreshTokenFlow());
            CreateToken(fixture, jwt => jwt.Audience = "baaaad");
            Assert.NotNull(fixture.Token);

            var transaction = await this.QueryAsync(server, fixture.Client.ClientId, fixture.Token.RefreshToken);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidToken, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_invalid_scope()
        {
            var fixture = new ServerFixture();
            var server = fixture.CreateServer(builder => builder.AddRefreshTokenFlow());
            CreateToken(fixture, jwt => jwt.Claims.Add("scope", "baaaad"));
            Assert.NotNull(fixture.Token);

            var transaction = await this.QueryAsync(server, fixture.Client.ClientId, fixture.Token.RefreshToken);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidScope, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_nominal()
        {
            var fixture = new ServerFixture();
            var server = fixture.CreateServer(builder => builder.AddRefreshTokenFlow());
            CreateToken(fixture, jwt => jwt.Claims.Add("scope", fixture.Client.Scope));
            Assert.NotNull(fixture.Token);

            var transaction = await this.QueryAsync(server, fixture.Client.ClientId, fixture.Token.RefreshToken);
            Assert.Equal(HttpStatusCode.OK, transaction.Response.StatusCode);

            var payload = fixture.Read(transaction.ResponseText, true);
            Assert.NotNull(payload);
            Assert.Equal("John Doe", payload["unique_name"]?.ToString());
            Assert.Equal("123", payload["nameid"]?.ToString());
            Assert.Equal(fixture.Client.Audience, payload["aud"]?.ToString());
            Assert.Equal(fixture.Client.Scope, payload["scope"]?.ToString());
            Assert.Equal(fixture.Options.Issuer, payload["iss"]?.ToString());
        }

        [Fact]
        public void Empty_tokenEndpoint_throws_exception() => Assert.Throws<ArgumentException>(() => this.Fixture.CreateServer(builder => builder.AddClientCredentialsGrantFlow(), options => options.TokenEndpointPath = null));
    }
}
