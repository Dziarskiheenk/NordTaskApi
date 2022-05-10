using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NordTaskApi.Functions.Common.Auth;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Xunit;

namespace NordTaskApi.Functions.Common.Tests
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task ShouldNotAuthorizeWithoutAuthorizeHeader()
        {
            var config = new Dictionary<string, string>
            {
                { "Authorization_issuerUrl",string.Empty },
                { "Authorization_audience",string.Empty }
            };
            var configMock = new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();

            var tokenHandlerMock = new Mock<JwtSecurityTokenHandler>();
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var oidcWrapperMock = new Mock<IOIDCConfigurationManagerWrapper>();

            var authService = new AuthService(configMock, tokenHandlerMock.Object, contextAccessorMock.Object, oidcWrapperMock.Object);

            var headers = new HeaderDictionary();
            var authResponse = await authService.Authorize(headers);

            Assert.False(authResponse);
        }

        [Fact]
        public async Task ShouldNotAuthorizeWithEmptyAuthHeader()
        {
            var config = new Dictionary<string, string>
            {
                { "Authorization_issuerUrl",string.Empty },
                { "Authorization_audience",string.Empty }
            };
            var configMock = new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();

            var tokenHandlerMock = new Mock<JwtSecurityTokenHandler>();
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var oidcWrapperMock = new Mock<IOIDCConfigurationManagerWrapper>();

            var authService = new AuthService(configMock, tokenHandlerMock.Object, contextAccessorMock.Object, oidcWrapperMock.Object);

            var headers = new HeaderDictionary();
            headers.Add("Authorization", new StringValues(string.Empty));
            var authResponse = await authService.Authorize(headers);

            Assert.False(authResponse);
        }

        [Theory]
        [InlineData("Bearer ")]
        [InlineData(" token")]
        [InlineData("Bear token")]
        [InlineData("Bearer token", "Bearer token2")]
        public async Task ShouldNotAuthorizeWithIncorrectHeader(params string[] authHeader)
        {
            var config = new Dictionary<string, string>
            {
                { "Authorization_issuerUrl",string.Empty },
                { "Authorization_audience",string.Empty }
            };
            var configMock = new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();

            var tokenHandlerMock = new Mock<JwtSecurityTokenHandler>();
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var oidcWrapperMock = new Mock<IOIDCConfigurationManagerWrapper>();

            var authService = new AuthService(configMock, tokenHandlerMock.Object, contextAccessorMock.Object, oidcWrapperMock.Object);

            var headers = new HeaderDictionary();
            headers.Add("Authorization", new StringValues(authHeader));
            var authResponse = await authService.Authorize(headers);

            Assert.False(authResponse);
        }

        [Fact]
        public async Task ShouldNotReturnValidResponseWhenTokenIsInvalid()
        {
            var config = new Dictionary<string, string>
            {
                { "Authorization_issuerUrl",string.Empty },
                { "Authorization_audience",string.Empty }
            };
            var configMock = new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();

            var tokenHandlerMock = new Mock<JwtSecurityTokenHandler>();
            tokenHandlerMock
                .Setup(t => t.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<TokenValidationParameters>()))
                .Returns(Task.FromResult(new TokenValidationResult { IsValid = false }));
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var oidcWrapperMock = new Mock<IOIDCConfigurationManagerWrapper>();

            var authService = new AuthService(configMock, tokenHandlerMock.Object, contextAccessorMock.Object, oidcWrapperMock.Object);

            var headers = new HeaderDictionary();
            headers.Add("Authorization", new StringValues("Bearer token"));
            var authResponse = await authService.Authorize(headers);

            Assert.False(authResponse);
        }
    }
}