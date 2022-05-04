using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using NordTaskApi.Common.Exceptions;

namespace NordTaskApi.Functions.Common.Auth
{
    public class AuthService : IAuthService
    {
        private readonly string validAudience;
        private readonly string issuerUrl;
        private readonly JwtSecurityTokenHandler jwtSecurityTokenHandler;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IOIDCConfigurationManagerWrapper configurationManagerWrapper;

        public AuthService(IConfiguration configuration, JwtSecurityTokenHandler jwtSecurityTokenHandler, IHttpContextAccessor httpContextAccessor, IOIDCConfigurationManagerWrapper configurationManagerWrapper)
        {
            this.jwtSecurityTokenHandler = jwtSecurityTokenHandler;
            this.httpContextAccessor = httpContextAccessor;
            var issuerUrl = configuration.GetSection("Authorization_issuerUrl").Value;
            validAudience = configuration.GetSection("Authorization_audience").Value;
            if (!issuerUrl.EndsWith('/'))
            {
                issuerUrl = $"{issuerUrl}/";
            }
            this.issuerUrl = issuerUrl;
            this.configurationManagerWrapper = configurationManagerWrapper;
        }

        public async Task<bool> Authorize(IHeaderDictionary headers)
        {
            var token = GetToken(headers);
            if (token == null)
            {
                return false;
            }

            ICollection<SecurityKey> issuerSigningKeys;
            try
            {
                issuerSigningKeys = await configurationManagerWrapper.GetSigningKeys();
            }
            catch (Exception ex)
            {
                throw new AuthenticationException($"Error occured during OIDC configuration obtaining: {ex.Message}", ex);
            }

            var validationParameters = new TokenValidationParameters
            {
                ValidIssuer = issuerUrl,
                IssuerSigningKeys = issuerSigningKeys,
                ValidAudience = validAudience,
                NameClaimType = ClaimTypes.Email
            };
            TokenValidationResult res;
            try
            {
                res = await jwtSecurityTokenHandler.ValidateTokenAsync(token, validationParameters);
            }
            catch (Exception ex)
            {
                throw new AuthenticationException($"Error occured during token validation: {ex.Message}", ex);
            }

            if (res.IsValid)
            {
                httpContextAccessor.HttpContext.User = new ClaimsPrincipal(res.ClaimsIdentity);
            }

            return res.IsValid;
        }

        private string? GetToken(IHeaderDictionary headers)
        {
            try
            {
                var authHeaders = headers.ContainsKey("Authorization") ? headers["Authorization"] : throw new ArgumentException("Incorrect Authorization header.", nameof(headers));
                if (authHeaders.Count != 1)
                {
                    throw new ArgumentException("Incorrect Authorization header.", nameof(headers));
                }

                var authHeader = string.IsNullOrWhiteSpace(authHeaders.First()) ? throw new ArgumentException("Incorrect Authorization header.", nameof(headers)) : authHeaders.First();
                if (!authHeader.StartsWith("Bearer ", StringComparison.InvariantCulture))
                {
                    throw new ArgumentException("Incorrect Authorization header.", nameof(headers));
                }

                var split = authHeader.Split("Bearer ");
                var token = split.Length != 2 ? throw new ArgumentException("Incorrect Authorization header.", nameof(headers)) : split[1];

                return token;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}