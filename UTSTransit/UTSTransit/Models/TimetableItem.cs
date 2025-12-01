namespace UTSTransit.Models
{
    public class TimetableItem
    {
        public string RouteName { get; set; } = string.Empty;
        public string DepartureTime { get; set; } = string.Empty;
        public string Status { get; set; } = "On Time"; // On Time, Delayed, Cancelled
        public Color StatusColor => Status == "On Time" ? Colors.Green : Colors.Red;
    }
}
