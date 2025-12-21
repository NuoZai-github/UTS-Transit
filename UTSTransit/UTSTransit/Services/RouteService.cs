// using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using UTSTransit.Models;

namespace UTSTransit.Services
{
    public class RouteService
    {
        public List<RouteInfo> GetRoutes()
        {
            return new List<RouteInfo>
            {
                new RouteInfo
                {
                    Name = "Route A (Dorm -> Campus)",
                    RouteColor = Colors.Blue,
                    Coordinates = new List<Location>
                    {
                        // Route A: Hostel -> Campus (Follows Jln Wawasan)
                        new Location(2.3420, 111.8318), // Hostel Start
                        new Location(2.3431, 111.8317), // Exit Hostel North
                        new Location(2.3435, 111.8340), // Jln Wawasan West
                        new Location(2.3433, 111.8365), // Jln Wawasan Mid
                        new Location(2.3426, 111.8386), // Curve
                        new Location(2.3418, 111.8405), // Pre-Roundabout
                        new Location(2.3415, 111.8417), // Roundabout
                        new Location(2.3415, 111.8424), // Enter Campus
                        new Location(2.3413, 111.8435), // Campus Road
                        new Location(2.3417, 111.8442)  // Campus Main
                    }
                },
                new RouteInfo
                {
                    Name = "Route B (Campus -> Hostel)",
                    RouteColor = Colors.Red,
                    Coordinates = new List<Location>
                    {
                        // Route B: Campus -> Hostel (Reverse)
                        new Location(2.3417, 111.8442),
                        new Location(2.3413, 111.8435),
                        new Location(2.3415, 111.8424),
                        new Location(2.3415, 111.8417),
                        new Location(2.3418, 111.8405),
                        new Location(2.3426, 111.8386),
                        new Location(2.3433, 111.8365),
                        new Location(2.3435, 111.8340),
                        new Location(2.3431, 111.8317),
                        new Location(2.3420, 111.8318)
                    }
                }
            };
        }


    }
}
