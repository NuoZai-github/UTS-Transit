namespace UTSTransit.Models
{
    public class TimetableItem
    {
        public Guid ScheduleId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string DepartureTime { get; set; } = string.Empty;
        public string Status { get; set; } = "On Time"; // On Time, Delayed, Cancelled
        public Color StatusColor => Status == "On Time" ? Colors.Green : Colors.Red;

        // Booking Support
        public bool IsBooked { get; set; }
        public string ActionButtonText => IsBooked ? "Booked" : "Book";
        public Color ActionButtonColor => IsBooked ? Colors.Gray : Colors.Blue;
        public bool CanBook => !IsBooked;
    }
}
