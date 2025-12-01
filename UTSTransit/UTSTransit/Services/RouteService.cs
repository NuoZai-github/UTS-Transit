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

        public List<TimetableItem> GetTimetable()
        {
            return new List<TimetableItem>
            {
                new TimetableItem { RouteName = "Route A", DepartureTime = "08:00 AM", Status = "On Time" },
                new TimetableItem { RouteName = "Route B", DepartureTime = "08:15 AM", Status = "On Time" },
                new TimetableItem { RouteName = "Route A", DepartureTime = "08:30 AM", Status = "Delayed" },
                new TimetableItem { RouteName = "Route B", DepartureTime = "08:45 AM", Status = "On Time" },
                new TimetableItem { RouteName = "Route A", DepartureTime = "09:00 AM", Status = "On Time" },
                new TimetableItem { RouteName = "Route B", DepartureTime = "09:15 AM", Status = "On Time" },
                new TimetableItem { RouteName = "Route A", DepartureTime = "09:30 AM", Status = "On Time" },
                new TimetableItem { RouteName = "Route B", DepartureTime = "09:45 AM", Status = "Cancelled" },
                new TimetableItem { RouteName = "Route A", DepartureTime = "10:00 AM", Status = "On Time" },
                new TimetableItem { RouteName = "Route B", DepartureTime = "10:15 AM", Status = "On Time" },
            };
        }
    }
}
