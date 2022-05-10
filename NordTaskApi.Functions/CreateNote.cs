using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NordTaskApi.Common.Exceptions;
using NordTaskApi.Common.Models;
using NordTaskApi.Common.Services;
using NordTaskApi.Functions.Common.Auth;
using System.IO;
using System.Threading.Tasks;

namespace NordTaskApi.Functions
{
    public class CreateNote
    {
        private readonly IAuthService authService;
        private readonly INotesService notesService;
        private readonly IHttpContextAccessor httpContextAccessor;
        public CreateNote(IAuthService authService, INotesService notesService, IHttpContextAccessor httpContextAccessor)
        {
            this.authService = authService;
            this.notesService = notesService;
            this.httpContextAccessor = httpContextAccessor;
        }

        [FunctionName("CreateNote")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "notes")] HttpRequest req,
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
            var userName = httpContextAccessor.HttpContext.User.Identity.Name;

            var createdNote = await notesService.CreateNote(note, userName);

            return new CreatedResult($"{UriHelper.GetEncodedUrl(req)}/{createdNote.Id}", createdNote);
        }
    }
}
