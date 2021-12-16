using NordTaskApi.Models;

namespace NordTaskApi.Repositories
{
    public interface INotesRepository
    {
        Task<IEnumerable<Note>> GetNotes(string userId);
        Task UpdateNote(Note note, string userId);
        Task<Note> CreateNote(Note note);
        Task DeleteNote(Guid id, string userId);
        Task DeleteNoteShare(Guid noteId, string userId);
    }
}
