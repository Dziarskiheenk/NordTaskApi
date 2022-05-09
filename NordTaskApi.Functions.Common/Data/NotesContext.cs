using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NordTaskApi.Common.Models;

namespace NordTaskApi.Common.Data
{
    public class NotesContext : DbContext
    {
        public DbSet<Note> Notes { get; set; }
        public DbSet<NoteShare> NoteShares { get; set; }

        public NotesContext() : base()
        { }

        public NotesContext(DbContextOptions<NotesContext> options)
            : base(options)
        { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Note>().HasKey(n => n.Id);
            modelBuilder.Entity<Note>().Property(n => n.CreatedAt).IsRequired();
            modelBuilder.Entity<Note>().Property(n => n.OwnedBy).IsRequired();
            modelBuilder.Entity<Note>().Property(n => n.Content).IsRequired();
            modelBuilder.Entity<Note>().Ignore(n => n.SharedWithEmails);

            modelBuilder.Entity<NoteShare>().HasKey(ns => new { ns.NoteId, ns.UserEmail });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer();
            base.OnConfiguring(optionsBuilder);
        }
    }

    public class NotesContextFactory : IDesignTimeDbContextFactory<NotesContext>
    {
        public NotesContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NotesContext>();
            optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("NordTaskApiDesignConnectionSring"));

            return new NotesContext(optionsBuilder.Options);
        }
    }
}
