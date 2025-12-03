using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace UTSTransit.Models
{
    [Table("trip_passengers")]
    public class TripPassenger : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("trip_id")]
        public int TripId { get; set; }

        [Column("student_id")]
        public Guid StudentId { get; set; }

        [Column("boarded_at")]
        public DateTime BoardedAt { get; set; }
    }
}
