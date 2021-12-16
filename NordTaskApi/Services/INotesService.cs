using NordTaskApi.Models;

namespace NordTaskApi.Services
{
    public interface INotesService
    {
        Task<IEnumerable<Note>> GetNotes(string userId);
        Task UpdateNote(Note note, string userId);
        Task<Note> CreateNote(Note note, string userId);
        Task DeleteNote(Guid id, string userId);
    }
}
