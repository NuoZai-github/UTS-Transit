using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace UTSTransit.Models
{
    [Table("bus_locations")]
    public class BusLocation : BaseModel
    {
        [PrimaryKey("driver_id", false)]
        public string DriverId { get; set; }

        [Column("route_name")]
        public string RouteName { get; set; }

        [Column("latitude")]
        public double Latitude { get; set; }

        [Column("longitude")]
        public double Longitude { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; }
    }
}