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
                        // Route A: Hostel -> Campus (Corrected Road Path)
                        // Keeping latitude closer to 2.3420 to prevent "V" shape dip
                        new Location(2.3420, 111.8318), // Hostel Start
                        new Location(2.3419, 111.8325), // Exit Hostel
                        new Location(2.3419, 111.8340), // Along Jalan Teku
                        new Location(2.3419, 111.8360),
                        new Location(2.3418, 111.8380), // Approaching Roundabout
                        new Location(2.3416, 111.8395), // Roundabout / Junction area
                        new Location(2.3415, 111.8410), // Entering Jalan University
                        new Location(2.3412, 111.8425), // Campus Driveway
                        new Location(2.3415, 111.8435), // Campus Road
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
                        new Location(2.3415, 111.8435),
                        new Location(2.3412, 111.8425),
                        new Location(2.3415, 111.8410),
                        new Location(2.3416, 111.8395),
                        new Location(2.3418, 111.8380),
                        new Location(2.3419, 111.8360),
                        new Location(2.3419, 111.8340),
                        new Location(2.3419, 111.8325),
                        new Location(2.3420, 111.8318)
                    }
                }
            };
        }


    }
}
