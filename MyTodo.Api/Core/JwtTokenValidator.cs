using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using HttpRequestData = Microsoft.Azure.Functions.Worker.Http.HttpRequestData;

namespace MyTodo.Api.Core
{

    public class JwtTokenValidator : IJwtTokenValidator
    {
        private readonly IConfiguration _configuration;
        private readonly string _wellKnownEndpoint;
        private readonly string _audience;
        private readonly string _instance;
        private static ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

        public JwtTokenValidator(IConfiguration configuration)
        {
            _configuration = configuration;
            _audience = _configuration["AzureAd:ClientId"];
            _instance = _configuration["AzureAd:Instance"];
            _wellKnownEndpoint = $"{_instance}/.well-known/openid-configuration";
        }

        public async Task<JwtTokenValidationResult> Validate(HttpRequestData httpRequest)
        {
            var authHeader = httpRequest.Headers.Contains("Authorization");

            if (authHeader == false) return new JwtTokenValidationResult(false);

            var token = httpRequest.Headers.Single(x => x.Key == "Authorization").Value.Single();
            token = token.Split(" ")[1];

            var oidcWellknownEndpoints = await GetOidcWellKnownConfiguration();

            var validationParameters = new TokenValidationParameters
            {
                RequireSignedTokens = true,
                ValidAudience = _audience,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKeys = oidcWellknownEndpoints.SigningKeys,
                ValidIssuer = oidcWellknownEndpoints.Issuer
            };

            SecurityToken securityToken;
            var handler = new JwtSecurityTokenHandler();

            var claimsPrincipal = handler.ValidateToken(token, validationParameters, out securityToken);

            return new JwtTokenValidationResult(true, claimsPrincipal);
        }

        private async Task<OpenIdConnectConfiguration> GetOidcWellKnownConfiguration()
        {
            if (_configurationManager == null)
            {
                _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(_wellKnownEndpoint, new OpenIdConnectConfigurationRetriever());
            }

            return await _configurationManager.GetConfigurationAsync();
        }
    }
}
