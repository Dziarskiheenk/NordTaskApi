using NordTaskApi.Models;
using NordTaskApi.Repositories;

namespace NordTaskApi.Services
{
    public class NotesService : INotesService
    {
        private readonly INotesRepository notesRepository;

        public NotesService(INotesRepository notesRepository)
        {
            this.notesRepository = notesRepository;
        }

        public async Task<Note> CreateNote(Note note, string userId)
        {
            note.Id = Guid.Empty;
            note.OwnedBy = userId;
            note.CreatedAt = DateTime.UtcNow;

            if (note.SharedWithEmails is not null)
            {
                note.SharedWith = note.SharedWithEmails.Select(s => new NoteShare { NoteId = note.Id, UserEmail = s }).ToList();
            }

            note = await notesRepository.CreateNote(note);

            return note;
        }

        public Task DeleteNote(Guid id, string userId) =>
            notesRepository.DeleteNote(id, userId);

        public async Task<IEnumerable<Note>> GetNotes(string userId)
        {
            var notes = await notesRepository.GetNotes(userId);
            notes.ToList().ForEach(n =>
            {
                if (n.SharedWith is not null)
                    n.SharedWithEmails = n.SharedWith.Select(sw => sw.UserEmail!).ToList();
            });
            return notes;
        }

        public async Task UpdateNote(Note note, string userId) =>
            await notesRepository.UpdateNote(note, userId);
    }
}
