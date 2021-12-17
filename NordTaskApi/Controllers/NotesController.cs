using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NordTaskApi.Exceptions;
using NordTaskApi.Models;
using NordTaskApi.Services;

namespace NordTaskApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly INotesService notesService;

        private string UserId
        {
            get
            {
                return User!.Identity!.Name!;
            }
        }

        public NotesController(INotesService notesService)
        {
            this.notesService = notesService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await notesService.GetNotes(UserId!));
        }

        [HttpGet("{id}/Content")]
        public async Task<IActionResult> GetProtectedContent([FromRoute] Guid id, [FromQuery] string password)
        {
            if (id == Guid.Empty || string.IsNullOrEmpty(password))
            {
                return BadRequest();
            }

            try
            {
                var content = await notesService.GetProtectedContent(id, password, UserId);
                return Ok(content);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedException)
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Note note)
        {
            var createdNote = await notesService.CreateNote(note, UserId!);
            return CreatedAtAction("Get", new { id = createdNote.Id }, createdNote);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] Note note)
        {
            if (id != note.Id)
            {
                return BadRequest();
            }

            try
            {
                await notesService.UpdateNote(note, UserId!);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedException)
            {
                return Unauthorized();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await notesService.DeleteNote(id, UserId!);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedException)
            {
                return Unauthorized();
            }
        }

        [HttpDelete("SharedNotes/{id}")]
        public async Task<IActionResult> DeleteShare(Guid id)
        {
            try
            {
                await notesService.DeleteNoteShare(id, UserId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
