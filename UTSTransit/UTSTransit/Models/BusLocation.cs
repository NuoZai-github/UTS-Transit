using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace UTSTransit.Models
{
    [Table("active_trips")]
    public class BusLocation : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }

        [Column("driver_id")]
        public string DriverId { get; set; }

        [Column("route_name")]
        public string RouteName { get; set; }

        [Column("latitude")]
        public double Latitude { get; set; }

        [Column("longitude")]
        public double Longitude { get; set; }

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; }
    }
}