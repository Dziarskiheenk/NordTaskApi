using NordTaskApi.Common.Exceptions;
using NordTaskApi.Common.Models;
using NordTaskApi.Common.Repositories;
using System.Text;

namespace NordTaskApi.Common.Services
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

        public async Task DeleteNote(Guid id, string userId)
        {
            var entry = (await notesRepository
                .GetNotes(userId, CancellationToken.None))
                .FirstOrDefault(n => n.Id == id);
            if (entry is null)
            {
                throw new KeyNotFoundException();
            }
            if (entry.OwnedBy != userId)
            {
                throw new UnauthorizedException();
            }
            await notesRepository.DeleteNote(id);
        }

        public async Task DeleteNoteShare(Guid noteId, string userId)
        {
            var entry = (await notesRepository.GetNoteShares(userId, CancellationToken.None))
                .FirstOrDefault(ns => ns.NoteId == noteId);
            if (entry is null)
            {
                throw new KeyNotFoundException();
            }
            await notesRepository.DeleteNoteShare(noteId, userId);
        }

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

            var entry = (await notesRepository.GetNotes(userId, CancellationToken.None))
                .FirstOrDefault(n=>n.Id==note.Id);
            if (entry is null)
            {
                throw new KeyNotFoundException();
            }
            var passwordsEqual = (string.IsNullOrWhiteSpace(note.Password) && string.IsNullOrWhiteSpace(entry.Password)) ||
                note.Password == entry.Password;
            if (entry.OwnedBy != userId || !passwordsEqual)
            {
                throw new UnauthorizedException();
            }

            entry.Title = note.Title;
            entry.Content = note.Content;
            entry.SharedWith = (await notesRepository.GetNoteShares(userId,CancellationToken.None))
                .Where(ns => ns.NoteId == note.Id)
                .ToList();
            entry.SharedWith.RemoveAll(entryShared => note.SharedWith is null || !note.SharedWith.Any(sw => sw.UserEmail == entryShared.UserEmail));
            if (note.SharedWith is not null)
            {
                entry.SharedWith.AddRange(note.SharedWith.Where(sw => !entry.SharedWith.Any(entryShare => entryShare.UserEmail == sw.UserEmail)));
            }

            await notesRepository.UpdateNote(note, userId);
        }

        public async Task<string?> GetProtectedContent(Guid noteId, string base64password, string userId)
        {
            byte[] data = Convert.FromBase64String(base64password);
            var password = Encoding.UTF8.GetString(data);
            var entry = await notesRepository.GetNote(noteId);
            var shares = await notesRepository.GetNoteShares(userId, CancellationToken.None);
            if (entry is null)
            {
                throw new KeyNotFoundException();
            }
            if (entry.Password != password || (entry.OwnedBy != userId && !shares.Any(s => s.UserEmail == userId)))
            {
                throw new UnauthorizedException();
            }

            return entry.Content;
        }
    }
}
