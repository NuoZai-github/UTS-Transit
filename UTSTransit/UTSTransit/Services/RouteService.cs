using Microsoft.Maui.Controls.Maps;
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
                        // Yura Mudang (Dorm)
                        new Location(-33.8806, 151.2030),
                        // Harris St
                        new Location(-33.8815, 151.2025),
                        new Location(-33.8825, 151.2018),
                        // Broadway
                        new Location(-33.8830, 151.2010),
                        // UTS Tower
                        new Location(-33.8832, 151.2008)
                    }
                },
                new RouteInfo
                {
                    Name = "Route B (Campus -> Moore Park)",
                    RouteColor = Colors.Red,
                    Coordinates = new List<Location>
                    {
                        // UTS Tower
                        new Location(-33.8832, 151.2008),
                        // Central Station area
                        new Location(-33.8840, 151.2050),
                        new Location(-33.8860, 151.2080),
                        // Cleveland St
                        new Location(-33.8900, 151.2150),
                        // Anzac Parade
                        new Location(-33.8950, 151.2200),
                        // Moore Park
                        new Location(-33.8988, 151.2227)
                    }
                }
            };
        }
    }
}
