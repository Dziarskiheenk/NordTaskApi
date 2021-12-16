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
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Note>> GetNotes(string userId)
        {
            var sharedNotes = await context.NoteShares
                .Where(ns => ns.UserEmail == userId)
                .Select(ns => ns.NoteId)
                .ToListAsync();
            return await context.Notes
                .Where(n => n.OwnedBy == userId || sharedNotes.Contains(n.Id))
                .Include(n => n.SharedWith)
                .ToListAsync();
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
            context.Entry(note).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }
    }
}
