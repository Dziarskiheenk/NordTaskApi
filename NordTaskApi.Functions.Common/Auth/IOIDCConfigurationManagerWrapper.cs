using Microsoft.IdentityModel.Tokens;

namespace NordTaskApi.Functions.Common.Auth
{
    public interface IOIDCConfigurationManagerWrapper
    {
        Task<ICollection<SecurityKey>> GetSigningKeys();
    }
}