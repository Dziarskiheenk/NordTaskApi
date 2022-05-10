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
    public class RemoveNote
    {
        private readonly IAuthService authService;
        private readonly INotesService notesService;
        private readonly IHttpContextAccessor httpContextAccessor;
        public RemoveNote(IAuthService authService, INotesService notesService, IHttpContextAccessor httpContextAccessor)
        {
            this.authService = authService;
            this.notesService = notesService;
            this.httpContextAccessor = httpContextAccessor;
        }

        [FunctionName("RemoveNote")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "notes/{noteId}")] HttpRequest req,
            Guid noteId,
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

            var userName = httpContextAccessor.HttpContext.User.Identity.Name;

            try
            {
                await notesService.DeleteNote(noteId, userName);
                return new NoContentResult();
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
