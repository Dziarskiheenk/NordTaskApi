using System.Text.Json.Serialization;

namespace NordTaskApi.Models
{
    public class Note
    {
        public Guid Id { get; set; }
        public string? OwnedBy { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        [JsonIgnore]
        public List<NoteShare>? SharedWith { get; set; }
        public List<string>? SharedWithEmails { get; set; }
    }
}
