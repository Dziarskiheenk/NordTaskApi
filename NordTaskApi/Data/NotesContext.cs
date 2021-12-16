using Microsoft.EntityFrameworkCore;
using NordTaskApi.Models;

namespace NordTaskApi.Data
{
    public class NotesContext : DbContext
    {
        public DbSet<Note> Notes { get; set; }
        public DbSet<NoteShare> NoteShares { get; set; }

        public NotesContext(DbContextOptions<NotesContext> options)
            : base(options)
        { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Note>().HasKey(n => n.Id);
            modelBuilder.Entity<Note>().Property(n => n.CreatedAt).IsRequired();
            modelBuilder.Entity<Note>().Property(n => n.OwnedBy).IsRequired();
            modelBuilder.Entity<Note>().Ignore(n => n.SharedWithEmails);

            modelBuilder.Entity<NoteShare>().HasKey(ns => new { ns.NoteId, ns.UserEmail });
        }
    }
}
