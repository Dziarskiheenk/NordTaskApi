using Microsoft.EntityFrameworkCore;
using NordTaskApi.Data;
using NordTaskApi.Exceptions;
using NordTaskApi.Models;

namespace NordTaskApi.Repositories
{
    public class NotesRepository : INotesRepository
    {
        private readonly NotesContext context;

        public NotesRepository(NotesContext context)
        {
            this.context = context;
        }

        public async Task<Note> CreateNote(Note note)
        {
            await context.Notes.AddAsync(note);
            await context.SaveChangesAsync();
            return note;
        }

        public async Task DeleteNote(Guid id)
        {
            context.Notes.Remove(context.Notes.First(x => x.Id == id));
            await context.SaveChangesAsync();
        }

        public async Task DeleteNoteShare(Guid noteId, string userId)
        {
            context.Remove(context.NoteShares.First(ns => ns.NoteId == noteId && ns.UserEmail == userId));
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Note>> GetNotes(string userId, CancellationToken cancellationToken)
        {
            var sharedNotes = (await GetNoteShares(userId, cancellationToken))
                .Select(ns => ns.NoteId);
            var notes = await context.Notes
                .Where(n => n.OwnedBy == userId || sharedNotes.Contains(n.Id))
                .Where(n => !n.ExpiresAt.HasValue || n.ExpiresAt >= DateTime.UtcNow)
                .Include(n => n.SharedWith)
                .ToListAsync(cancellationToken);
            notes.ForEach(n => n.CreatedAt = DateTime.SpecifyKind(n.CreatedAt, DateTimeKind.Utc));
            return notes;
        }

        public async Task<IEnumerable<NoteShare>> GetNoteShares(string userId, CancellationToken cancellationToken)
        {
            var sharedNotes = await context.NoteShares
                .Where(ns => ns.UserEmail == userId)
                .ToListAsync(cancellationToken);
            return sharedNotes;
        }

        public async Task UpdateNote(Note note, string userId)
        {
            var entry = await context.Notes.FindAsync(note.Id);
            if (entry is null)
            {
                throw new KeyNotFoundException();
            }
            if (entry.OwnedBy != userId || entry.Password != note.Password)
            {
                throw new UnauthorizedException();
            }

            entry.Title = note.Title;
            entry.Content = note.Content;

            entry.SharedWith = await context.NoteShares.Where(ns => ns.NoteId == note.Id).ToListAsync();
            entry.SharedWith.RemoveAll(entryShared => note.SharedWith is null || !note.SharedWith.Any(sw => sw.UserEmail == entryShared.UserEmail));
            if (note.SharedWith is not null)
            {
                entry.SharedWith.AddRange(note.SharedWith.Where(sw => !entry.SharedWith.Any(entryShare => entryShare.UserEmail == sw.UserEmail)));
            }

            context.Entry(entry).State = EntityState.Modified;

            await context.SaveChangesAsync();
        }

        public async Task<string?> GetProtectedContent(Guid id, string password, string userId)
        {
            var entry = await context.Notes.FindAsync(id);
            var shares = await context.NoteShares.Where(ns => ns.NoteId == id).ToListAsync();
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
