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
                        // Hostel (Kolej Laila Taib area)
                        new Location(2.3180, 111.8300),
                        // Jalan Teku
                        new Location(2.3160, 111.8290),
                        // UTS Campus
                        new Location(2.3134, 111.8283)
                    }
                },
                new RouteInfo
                {
                    Name = "Route B (Campus -> Hostel)",
                    RouteColor = Colors.Red,
                    Coordinates = new List<Location>
                    {
                        // UTS Campus
                        new Location(2.3134, 111.8283),
                        // Jalan Teku
                        new Location(2.3160, 111.8290),
                        // Hostel
                        new Location(2.3180, 111.8300)
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
