
using Microsoft.AspNetCore.Http;

namespace NordTaskApi.Functions.Common.Auth
{
    public interface IAuthService
    {
        Task<bool> Authorize(IHeaderDictionary headers);
    }
}