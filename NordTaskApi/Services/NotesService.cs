using NordTaskApi.Models;
using NordTaskApi.Repositories;
using System.Text;

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
            note.Password = note.GetDecodedUserPassword();

            if (note.SharedWithEmails is not null)
            {
                note.SharedWith = note.SharedWithEmails.Select(s => new NoteShare { NoteId = note.Id, UserEmail = s }).ToList();
            }

            note = await notesRepository.CreateNote(note);

            return note;
        }

        public async Task DeleteNote(Guid id, string userId) =>
            await notesRepository.DeleteNote(id, userId);

        public async Task DeleteNoteShare(Guid noteId, string userId) =>
            await notesRepository.DeleteNoteShare(noteId, userId);

        public async Task<IEnumerable<Note>> GetNotes(string userId, CancellationToken cancellationToken)
        {
            var notes = await notesRepository.GetNotes(userId, cancellationToken);
            notes.ToList().ForEach(n =>
            {
                if (n.SharedWith is not null)
                    n.SharedWithEmails = n.SharedWith.Select(sw => sw.UserEmail!).ToList();
                
                // TODO think about string encryption when saved with password instead of returning empty content
                if (n.IsPasswordProtected)
                    n.Content = string.Empty;
            });
            return notes;
        }

        public async Task UpdateNote(Note note, string userId)
        {
            if (note.SharedWithEmails is not null)
            {
                note.SharedWith = note.SharedWithEmails.Select(s => new NoteShare { NoteId = note.Id, UserEmail = s }).ToList();
            }

            note.Password = note.GetDecodedUserPassword();

            await notesRepository.UpdateNote(note, userId);
        }

        public async Task<string?> GetProtectedContent(Guid noteId, string base64password, string userId)
        {
            byte[] data = Convert.FromBase64String(base64password);
            var password = Encoding.UTF8.GetString(data);
            return await notesRepository.GetProtectedContent(noteId, password, userId);
        }
    }
}
