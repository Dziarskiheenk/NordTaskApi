using NordTaskApi.Models;

namespace NordTaskApi.Repositories
{
    public interface INotesRepository
    {
        Task<IEnumerable<Note>> GetNotes(string userId, CancellationToken cancellationToken);
        Task<IEnumerable<NoteShare>> GetNoteShares(string userId, CancellationToken cancellationToken);
        Task UpdateNote(Note note, string userId);
        Task<Note> CreateNote(Note note);
        Task DeleteNote(Guid id);
        Task DeleteNoteShare(Guid noteId, string userId);
        Task<string?> GetProtectedContent(Guid id, string password, string userId);
    }
}
