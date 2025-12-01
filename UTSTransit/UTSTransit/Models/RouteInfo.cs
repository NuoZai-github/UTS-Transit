using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Controls.Maps;

namespace UTSTransit.Models
{
    public class RouteInfo
    {
        public string Name { get; set; }
        public Color RouteColor { get; set; }
        public List<Location> Coordinates { get; set; }
    }
}
