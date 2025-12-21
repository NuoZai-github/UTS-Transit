namespace UTSTransit.Models
{
    public class TimetableItem
    {
        public Guid ScheduleId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string DepartureTime { get; set; } = string.Empty;
        public string Status { get; set; } = "On Time";
        
        public bool IsBooked { get; set; }
        public bool IsClosed { get; set; }
        public bool IsPast { get; set; } // NEW: true if departure time has passed
        public bool IsCancelled { get; set; }
        public Guid? BookingId { get; set; }

        // Explicit properties - set these in ViewModel
        public string ActionButtonText { get; set; } = "Book";
        public Color ActionButtonColor { get; set; } = Colors.Blue;
        public Color StatusColor { get; set; } = Colors.Green;
        public bool CanBook { get; set; } = true;
        public TextDecorations RouteTextDecoration { get; set; } = TextDecorations.None;
        public TextDecorations TimeTextDecoration { get; set; } = TextDecorations.None;
        public double ItemOpacity { get; set; } = 1.0;

        // Helper method to compute all display properties
        public void ComputeDisplayProperties()
        {
            // Check if cancelled by status OR if time has passed
            IsCancelled = Status == "Cancelled" || IsPast;

            // Status color
            if (IsCancelled || IsPast)
                StatusColor = Colors.Gray;
            else if (Status == "On Time" || Status == "Scheduled")
                StatusColor = Colors.Green;
            else
                StatusColor = Colors.Red;

            // Strikethrough and opacity for past/cancelled items
            if (IsCancelled || IsPast)
            {
                RouteTextDecoration = TextDecorations.Strikethrough;
                TimeTextDecoration = TextDecorations.Strikethrough;
                ItemOpacity = 0.5;
            }
            else
            {
                RouteTextDecoration = TextDecorations.None;
                TimeTextDecoration = TextDecorations.None;
                ItemOpacity = 1.0;
            }

            // Button text and color
            if (IsPast)
            {
                ActionButtonText = "Departed";
                ActionButtonColor = Colors.Gray;
                CanBook = false;
            }
            else if (Status == "Cancelled")
            {
                ActionButtonText = "Cancelled";
                ActionButtonColor = Colors.Gray;
                CanBook = false;
            }
            else if (IsClosed)
            {
                ActionButtonText = "Closed";
                ActionButtonColor = Colors.Gray;
                CanBook = false;
            }
            else if (IsBooked)
            {
                ActionButtonText = "Undo";
                ActionButtonColor = Colors.Orange;
                CanBook = true;
            }
            else
            {
                ActionButtonText = "Book";
                ActionButtonColor = Colors.Blue;
                CanBook = true;
            }
        }
    }
}

