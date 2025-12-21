using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace UTSTransit.Models
{
    [Table("schedules")]
    public class ScheduleItem : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("route_name")]
        public string RouteName { get; set; } = string.Empty;

        [Column("departure_time")]
        public TimeSpan DepartureTime { get; set; }

        [Column("day_type")]
        public string DayType { get; set; } = "Weekday";

        [Column("status")]
        public string Status { get; set; } = "Scheduled";

        [Column("special_date")]
        public DateTime? SpecialDate { get; set; }
    }
}
