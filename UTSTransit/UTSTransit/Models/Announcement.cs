using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace UTSTransit.Models
{
    [Table("announcements")]
    public class Announcement : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("is_urgent")]
        public bool IsUrgent { get; set; }

        public string DateString => CreatedAt.ToString("MMM dd, yyyy");
    }
}
