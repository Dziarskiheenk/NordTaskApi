using System.Text;
using System.Text.Json.Serialization;

namespace NordTaskApi.Models
{
    // TODO split it to JSON model and database entity model
    public class Note
    {
        public Guid Id { get; set; }
        public string? OwnedBy { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? UserPassword { private get; set; }

        [JsonIgnore]
        public string? Password { get; set; }

        [JsonIgnore]
        public List<NoteShare>? SharedWith { get; set; }
        public List<string>? SharedWithEmails { get; set; }

        public bool IsPasswordProtected
        {
            get => !string.IsNullOrEmpty(Password);
        }

        public string? GetUserPassword()
        {
            if (!string.IsNullOrEmpty(UserPassword))
            {
                byte[] data = Convert.FromBase64String(UserPassword);
                return Encoding.UTF8.GetString(data);
            }
            return string.Empty;
        }
    }
}
