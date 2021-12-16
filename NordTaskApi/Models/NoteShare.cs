using System.ComponentModel.DataAnnotations;

namespace NordTaskApi.Models
{
    public class NoteShare
    {
        public Guid NoteId { get; set; }
        public string? UserEmail { get; set; }
    }
}
