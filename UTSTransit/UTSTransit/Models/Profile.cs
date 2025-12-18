using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace UTSTransit.Models
{
    [Table("profiles")]
    public class Profile : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("role")]
        public string Role { get; set; } = string.Empty;

        [Column("student_id")]
        public string? StudentId { get; set; }

        [Column("ic_number")]
        public string? IcNumber { get; set; }

        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }
    }
}
