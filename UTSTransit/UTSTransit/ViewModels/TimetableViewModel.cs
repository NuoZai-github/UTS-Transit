using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using UTSTransit.Models;
using UTSTransit.Services;

namespace UTSTransit.ViewModels
{
    public partial class TimetableViewModel : ObservableObject
    {
        private readonly TransitService _transitService;

        public ObservableCollection<TimetableItem> Schedule { get; } = new();

        // Selected date: 0 = Today, 1 = Tomorrow
        [ObservableProperty]
        private int _selectedDateIndex = 0;

        // Tab colors based on selection
        public Color TodayTabColor => SelectedDateIndex == 0 ? GetPrimaryColor() : GetPageBackgroundColor();
        public Color TomorrowTabColor => SelectedDateIndex == 1 ? GetPrimaryColor() : GetPageBackgroundColor();
        public Color TodayTextColor => SelectedDateIndex == 0 ? Colors.White : GetSecondaryTextColor();
        public Color TomorrowTextColor => SelectedDateIndex == 1 ? Colors.White : GetSecondaryTextColor();

        // Empty view message
        [ObservableProperty]
        private string _emptyMessage = "All trips have departed. Please check again tomorrow.";

        public TimetableViewModel(TransitService transitService)
        {
            _transitService = transitService;
            LoadSchedule();
        }

        private Color GetPrimaryColor()
        {
            if (Application.Current.Resources.TryGetValue("Primary", out var color))
                return (Color)color;
            return Colors.Blue;
        }

        private Color GetPageBackgroundColor()
        {
            if (Application.Current.Resources.TryGetValue("PageBackground", out var color))
                return (Color)color;
            return Colors.LightGray;
        }

        private Color GetSecondaryTextColor()
        {
            if (Application.Current.Resources.TryGetValue("TextSecondary", out var color))
                return (Color)color;
            return Colors.Gray;
        }

        [RelayCommand]
        private void SelectToday()
        {
            SelectedDateIndex = 0;
            OnPropertyChanged(nameof(TodayTabColor));
            OnPropertyChanged(nameof(TomorrowTabColor));
            OnPropertyChanged(nameof(TodayTextColor));
            OnPropertyChanged(nameof(TomorrowTextColor));
            LoadSchedule();
        }

        [RelayCommand]
        private void SelectTomorrow()
        {
            SelectedDateIndex = 1;
            OnPropertyChanged(nameof(TodayTabColor));
            OnPropertyChanged(nameof(TomorrowTabColor));
            OnPropertyChanged(nameof(TodayTextColor));
            OnPropertyChanged(nameof(TomorrowTextColor));
            LoadSchedule();
        }

        [RelayCommand]
        private async Task Book(TimetableItem item)
        {
            if (item.IsClosed)
            {
                await Application.Current.MainPage.DisplayAlert("Closed", "Bookings close 10 minutes before departure.", "OK");
                return;
            }

            if (item.IsBooked)
            {
                if (item.BookingId == null) 
                {
                     await Application.Current.MainPage.DisplayAlert("Error", "Booking ID missing.", "OK");
                     return;
                }

                bool confirm = await Application.Current.MainPage.DisplayAlert("Cancel", "Undo this booking?", "Yes", "No");
                if (!confirm) return;

                var result = await _transitService.CancelBookingAsync(item.BookingId.Value);
                if (result.IsSuccess)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "Booking cancelled.", "OK");
                    LoadSchedule();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", result.Message, "OK");
                }
            }
            else
            {
                // Pass the correct date (today or tomorrow)
                DateTime bookingDate = SelectedDateIndex == 0 ? DateTime.Today : DateTime.Today.AddDays(1);
                var result = await _transitService.BookSlotAsync(item.ScheduleId, bookingDate);
                if (result.IsSuccess)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "Trip booked successfully!", "OK");
                    LoadSchedule();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", result.Message, "OK");
                }
            }
        }

        private async void LoadSchedule()
        {
            var schedules = await _transitService.GetSchedulesAsync();
            var myBookings = await _transitService.GetStudentBookingsAsync();
            
            // Determine the target date
            DateTime targetDate = SelectedDateIndex == 0 ? DateTime.Today : DateTime.Today.AddDays(1);
            DayOfWeek targetDayOfWeek = targetDate.DayOfWeek;
            
            System.Diagnostics.Debug.WriteLine($"[BOOKING] ===== BOOKING DATE DEBUG =====");
            System.Diagnostics.Debug.WriteLine($"[BOOKING] DateTime.Now: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            System.Diagnostics.Debug.WriteLine($"[BOOKING] DateTime.Today: {DateTime.Today:yyyy-MM-dd}");
            System.Diagnostics.Debug.WriteLine($"[BOOKING] TargetDate: {targetDate:yyyy-MM-dd}");
            System.Diagnostics.Debug.WriteLine($"[BOOKING] SelectedDateIndex: {SelectedDateIndex} ({(SelectedDateIndex == 0 ? "Today" : "Tomorrow")})");
            System.Diagnostics.Debug.WriteLine($"[BOOKING] Total bookings fetched: {myBookings.Count}");
            
            foreach (var b in myBookings)
            {
                System.Diagnostics.Debug.WriteLine($"[BOOKING] -> Booking: ScheduleId={b.ScheduleId}");
                System.Diagnostics.Debug.WriteLine($"[BOOKING]    BookingDate raw: {b.BookingDate}");
                System.Diagnostics.Debug.WriteLine($"[BOOKING]    BookingDate.Date: {b.BookingDate.Date:yyyy-MM-dd}");
                System.Diagnostics.Debug.WriteLine($"[BOOKING]    TargetDate.Date: {targetDate.Date:yyyy-MM-dd}");
                System.Diagnostics.Debug.WriteLine($"[BOOKING]    Match: {b.BookingDate.Date == targetDate.Date}");
            }
            
            // Filter bookings by the target date
            var bookingsForDate = myBookings.Where(b => b.BookingDate.Date == targetDate.Date).ToList();
            var bookingMap = bookingsForDate.ToDictionary(b => b.ScheduleId, b => b);
            
            System.Diagnostics.Debug.WriteLine($"[BOOKING] Bookings matching target date: {bookingsForDate.Count}");
            System.Diagnostics.Debug.WriteLine($"[BOOKING] ==============================");

            Schedule.Clear();
            var now = DateTime.Now;

            System.Diagnostics.Debug.WriteLine($"[SCHEDULE] SelectedDateIndex: {SelectedDateIndex}, TargetDate: {targetDate:dddd yyyy-MM-dd}, DayOfWeek: {targetDayOfWeek}");

            // Weekday/Weekend logic combined
            EmptyMessage = "No bus service available for this date.";
            bool isWeekend = targetDayOfWeek == DayOfWeek.Saturday || targetDayOfWeek == DayOfWeek.Sunday;

            foreach (var item in schedules)
            {
                bool shouldShow = false;

                // 1. Special Slots: Only show if valid for this specific date
                if (item.DayType == "Special")
                {
                    if (item.SpecialDate.HasValue && item.SpecialDate.Value.Date == targetDate.Date)
                    {
                        shouldShow = true;
                        System.Diagnostics.Debug.WriteLine($"[SCHEDULE] Found Special Event for {targetDate:MM-dd}");
                    }
                }
                // 2. Daily Slots: Only show on Weekdays (Mon-Fri)
                else if (!isWeekend)
                {
                    shouldShow = true;
                }

                if (!shouldShow) continue;

                var departureDateTime = targetDate.Add(item.DepartureTime);
                
                // For Today: Skip if past cutoff (10 mins before), UNLESS booked (so they can see past trips if needed, or maybe just active ones?)
                // Actually requirement says auto-close, usually implies they disappear or become unbookable.
                // Current logic was skipping past items. Let's keep skipping past items for "Available" list.
                // If the user has booked it, maybe we want to show it?
                // For now, let's stick to "hiding past trips" to keep list clean, as per previous logic.
                
                if (SelectedDateIndex == 0)
                {
                    var cutoffTime = departureDateTime.AddMinutes(-10);
                    if (now > cutoffTime)
                    {
                        // OPTIONAL: If booked, maybe show it? Current logic was skipping. 
                        // Let's stick to skipping for consistency unless requested.
                        System.Diagnostics.Debug.WriteLine($"[SCHEDULE] SKIPPING {item.RouteName} @ {departureDateTime:HH:mm} - Cutoff passed");
                        continue;
                    }
                }

                var booking = bookingMap.ContainsKey(item.Id) ? bookingMap[item.Id] : null;
                var isBooked = booking != null;

                var timetableItem = new TimetableItem
                {
                    ScheduleId = item.Id,
                    RouteName = item.RouteName,
                    DepartureTime = departureDateTime.ToString("hh:mm tt"),
                    Status = item.Status,
                    IsBooked = isBooked,
                    BookingId = booking?.Id,
                    IsClosed = false,
                    IsPast = false
                };
                
                timetableItem.ComputeDisplayProperties();
                
                System.Diagnostics.Debug.WriteLine($"[SCHEDULE] SHOWING {item.RouteName} @ {departureDateTime:HH:mm}");
                
                Schedule.Add(timetableItem);
            }
        }
    }
}

