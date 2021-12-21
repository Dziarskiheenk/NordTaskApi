using NordTaskApi.Models;

namespace NordTaskApi.Services
{
    public interface INotesService
    {
        Task<IEnumerable<Note>> GetNotes(string userId, CancellationToken cancellationToken);
        Task UpdateNote(Note note, string userId);
        Task<Note> CreateNote(Note note, string userId);
        Task DeleteNote(Guid id, string userId);
        Task DeleteNoteShare(Guid noteId, string userId);
        Task<string?> GetProtectedContent(Guid id, string password, string userId);
    }
}
