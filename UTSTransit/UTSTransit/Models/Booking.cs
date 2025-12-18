using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace UTSTransit.Models
{
    [Table("bookings")]
    public class Booking : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("schedule_id")]
        public Guid ScheduleId { get; set; }

        [Column("student_id")]
        public Guid StudentId { get; set; }

        [Column("booking_date")]
        public DateTime BookingDate { get; set; }

        [Column("status")]
        public string Status { get; set; } = "Booked";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Optional: Include Profile for joins if needed (though Supabase client joins can be tricky, we'll fetch separately usually)
    }
}
