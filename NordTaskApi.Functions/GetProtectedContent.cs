using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NordTaskApi.Common.Exceptions;
using NordTaskApi.Common.Services;
using NordTaskApi.Functions.Common.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NordTaskApi.Functions
{
    public class GetProtectedContent
    {
        private readonly IAuthService authService;
        private readonly INotesService notesService;
        private readonly IHttpContextAccessor httpContextAccessor;
        public GetProtectedContent(IAuthService authService, INotesService notesService, IHttpContextAccessor httpContextAccessor)
        {
            this.authService = authService;
            this.notesService = notesService;
            this.httpContextAccessor = httpContextAccessor;
        }

        [FunctionName("GetProtectedContent")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "notes/{id}/content")] HttpRequest req,
            Guid id,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                if (!await authService.Authorize(req.Headers))
                {
                    return new UnauthorizedResult();
                }
            }
            catch (AuthenticationException)
            {
                return new StatusCodeResult(503);
            }

            var password = req.Query["password"];
            if (id == Guid.Empty || string.IsNullOrEmpty(password))
            {
                return new BadRequestResult();
            }

            var userName = httpContextAccessor.HttpContext.User.Identity.Name;

            try
            {
                var content = await notesService.GetProtectedContent(id, password, userName);
                return new OkObjectResult(content);
            }
            catch (KeyNotFoundException)
            {
                return new NotFoundResult();
            }
            catch (UnauthorizedException)
            {
                return new UnauthorizedResult();
            }
        }
    }
}
