using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NordTaskApi.Common.Exceptions;
using NordTaskApi.Common.Models;
using NordTaskApi.Common.Services;
using NordTaskApi.Functions.Common.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NordTaskApi.Functions
{
    public class UpdateNote
    {
        private readonly IAuthService authService;
        private readonly INotesService notesService;
        private readonly IHttpContextAccessor httpContextAccessor;
        public UpdateNote(IAuthService authService, INotesService notesService, IHttpContextAccessor httpContextAccessor)
        {
            this.authService = authService;
            this.notesService = notesService;
            this.httpContextAccessor = httpContextAccessor;
        }

        [FunctionName("UpdateNote")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "notes/{noteId}")] HttpRequest req,
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

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Note note = JsonConvert.DeserializeObject<Note>(requestBody);
            if (note.Id != noteId)
            {
                return new BadRequestResult();
            }
            var userName = httpContextAccessor.HttpContext.User.Identity.Name;

            try
            {
                await notesService.UpdateNote(note, userName);
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
