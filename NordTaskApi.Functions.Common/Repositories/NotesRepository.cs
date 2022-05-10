using Microsoft.EntityFrameworkCore;
using NordTaskApi.Common.Data;
using NordTaskApi.Common.Models;

namespace NordTaskApi.Common.Repositories
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
                .AsNoTracking()
                .Where(n => n.OwnedBy == userId || sharedNotes.Contains(n.Id))
                .Where(n => !n.ExpiresAt.HasValue || n.ExpiresAt >= DateTime.UtcNow)
                .Include(n => n.SharedWith)
                .ToListAsync(cancellationToken);
            notes.ForEach(n => n.CreatedAt = DateTime.SpecifyKind(n.CreatedAt, DateTimeKind.Utc));
            return notes;
        }

        public async Task<Note?> GetNote(Guid id)
        {
            return await context.Notes.FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<IEnumerable<NoteShare>> GetNoteShares(string userId, CancellationToken cancellationToken)
        {
            var sharedNotes = await context.NoteShares
                .AsNoTracking()
                .Where(ns => ns.UserEmail == userId)
                .ToListAsync(cancellationToken);
            return sharedNotes;
        }

        public async Task UpdateNote(Note note, string userId)
        {
            var entry = context.Notes.Include(e => e.SharedWith).First(n => n.Id == note.Id);
            context.Entry(entry).CurrentValues.SetValues(note);
            entry.SharedWith = note.SharedWith;

            await context.SaveChangesAsync();
        }
    }
}
