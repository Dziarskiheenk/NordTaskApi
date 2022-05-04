using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace NordTaskApi.Functions.Common.Auth
{
    public class OIDCConfigurationManagerWrapper : IOIDCConfigurationManagerWrapper
    {
        private readonly ConfigurationManager<OpenIdConnectConfiguration> configurationManager;

        public OIDCConfigurationManagerWrapper(IConfiguration configuration)
        {
            var issuerUrl = configuration.GetSection("Authorization_issuerUrl").Value;
            if (!issuerUrl.EndsWith('/'))
            {
                issuerUrl = $"{issuerUrl}/";
            }
            configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{issuerUrl}.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever()
                );
        }

        public async Task<ICollection<SecurityKey>> GetSigningKeys()
        {
            var config = await configurationManager.GetConfigurationAsync();
            return config.SigningKeys;
        }
    }
}
