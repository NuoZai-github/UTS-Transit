using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;

namespace UTSTransit.Models
{
    [Table("profiles")]
    public class Profile : BaseModel
    {
        [PrimaryKey("id")]
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [Column("email")]
        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [Column("role")]
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;

        [Column("student_id")]
        [JsonProperty("student_id")]
        public string? StudentId { get; set; }

        [Column("ic_number")]
        [JsonProperty("ic_number")]
        public string? IcNumber { get; set; }

        [Column("avatar_url")]
        [JsonProperty("avatar_url")]
        public string? AvatarUrl { get; set; }

        [Column("full_name")]
        [JsonProperty("full_name")]
        public string? FullName { get; set; }

        [Column("updated_at")]
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
