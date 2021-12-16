using Microsoft.EntityFrameworkCore;
using NordTaskApi.Data;
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

        public async Task DeleteNote(Guid id, string userId)
        {
            var entry = await context.Notes.FirstOrDefaultAsync(n => n.Id == id);
            if (entry is null)
            {
                throw new KeyNotFoundException();
            }
            if (entry.OwnedBy != userId)
            {
                throw new InvalidOperationException();
            }
            context.Notes.Remove(entry);
            await context.SaveChangesAsync();
        }

        public async Task DeleteNoteShare(Guid noteId, string userId)
        {
            var entry = await context.NoteShares.FirstOrDefaultAsync(ns => ns.NoteId == noteId && ns.UserEmail == userId);
            if (entry is null)
            {
                throw new KeyNotFoundException();
            }
            context.Remove(entry);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Note>> GetNotes(string userId)
        {
            var sharedNotes = await context.NoteShares
                .Where(ns => ns.UserEmail == userId)
                .Select(ns => ns.NoteId)
                .ToListAsync();
            var notes = await context.Notes
                .Where(n => n.OwnedBy == userId || sharedNotes.Contains(n.Id))
                .Include(n => n.SharedWith)
                .ToListAsync();
            notes.ForEach(n => n.CreatedAt = DateTime.SpecifyKind(n.CreatedAt, DateTimeKind.Utc));
            return notes;
        }

        public async Task UpdateNote(Note note, string userId)
        {
            var entry = context.Entry(note).Entity;
            if (entry is null)
            {
                throw new KeyNotFoundException();
            }
            if (entry.OwnedBy != userId)
            {
                throw new InvalidOperationException();
            }

            var oldShares = await context.NoteShares
                .Where(ns => ns.NoteId == note.Id)
                .ToListAsync();
            var newShares = new List<NoteShare>();
            if (note.SharedWith is not null)
            {
                newShares = note.SharedWith.Where(sw => !oldShares.Any(os => os.UserEmail == sw.UserEmail)).ToList();
                oldShares = oldShares.Where(os => !note.SharedWith.Any(sw => sw.UserEmail == os.UserEmail)).ToList();
            }
            context.NoteShares.RemoveRange(oldShares);
            context.NoteShares.AddRange(newShares);
            
            context.Entry(note).State = EntityState.Modified;

            await context.SaveChangesAsync();
        }
    }
}
