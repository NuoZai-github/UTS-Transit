using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using UTSTransit.Services;

namespace UTSTransit.ViewModels
{
    public partial class DriverViewModel : ObservableObject
    {
        private readonly TransitService _transitService;
        private IDispatcherTimer? _timer;
        private bool _isSharing;

#pragma warning disable MVVMTK0045 // Partial properties are recommended for WinRT compatibility
        [ObservableProperty]
        private string _statusMessage = "Select a trip to view passengers";

        [ObservableProperty]
        private string _selectedRoute = "Please select your schedule first";

        [ObservableProperty]
        private string _selectedStatus = "Driving";

        public List<string> StatusOptions { get; } = new List<string>
        {
            "Driving",
            "Resting",
            "Arrived at Campus",
            "Arrived at Hostel",
            "Service Stopped"
        };

        [ObservableProperty]
        private bool _isBusy;

        // --- New Properties for Passenger View ---
        [ObservableProperty]
        private Models.ScheduleItem _selectedScheduleItem;

        public ObservableCollection<Models.ScheduleItem> Schedules { get; } = new();
        public ObservableCollection<Models.PassengerInfo> PassengerList { get; } = new();

#pragma warning restore MVVMTK0045

        public DriverViewModel(TransitService transitService)
        {
            _transitService = transitService;
            CheckDriverRole();
            LoadSchedules();
        }

        private async void LoadSchedules()
        {
            var items = await _transitService.GetSchedulesAsync();
            Schedules.Clear();
            
            var today = DateTime.Today; // Uses local system time, which is usually correct for the device context
            var isWeekend = today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday;

            foreach (var item in items)
            {
                bool shouldShow = false;

                // 1. Special Slots: Only show if valid for TODAY
                if (item.DayType == "Special")
                {
                    if (item.SpecialDate.HasValue && item.SpecialDate.Value.Date == today)
                    {
                        shouldShow = true;
                    }
                }
                // 2. Daily Slots: Only show on Weekdays (Mon-Fri)
                else if (!isWeekend)
                {
                    shouldShow = true;
                }

                if (shouldShow)
                {
                    // Optional: Sort by time? The API likely returns sorted by time, but we can rely on that.
                    Schedules.Add(item);
                }
            }
        }

        async partial void OnSelectedScheduleItemChanged(Models.ScheduleItem value)
        {
            if (value == null) return;
            IsBusy = true;
            SelectedRoute = value.RouteName; // Sync route name

            // Fetch students for TODAY
            var students = await _transitService.GetBookedStudentsForScheduleAsync(value.Id, DateTime.Today);
            PassengerList.Clear();
            if (students.Count == 0)
            {
                 StatusMessage = "No bookings for this trip yet.";
            }
            else
            {
                StatusMessage = $"{students.Count} student(s) booked.";
                foreach (var student in students)
                {
                    PassengerList.Add(student);
                }
            }
            IsBusy = false;
        }

        private void CheckDriverRole()
        {
            var role = _transitService.GetCurrentUserRole();
            if (role != "driver")
            {
                StatusMessage = "Access Denied: You are not registered as a driver.";
            }
        }

        [ObservableProperty]
        private string _buttonText = "Start Journey";

        [ObservableProperty]
        private Color _buttonColor = Colors.Green; // Default Green

        [RelayCommand]
        public async Task ToggleSharing()
        {
            var role = _transitService.GetCurrentUserRole();
            if (role != "driver")
            {
                StatusMessage = "Only drivers can share location.";
                return;
            }

            if (_isSharing)
            {
                // STOPPING
                StopLocationUpdates();
                StatusMessage = "Trip Ended";
                await _transitService.StopSharing();
                
                // Auto-update status and UI
                SelectedStatus = "Service Stopped";
                ButtonText = "Start Journey";
                ButtonColor = Colors.Green; // Back to Start
                _isSharing = false;
            }
            else
            {
                // STARTING
                // Check Permissions
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                if (status != PermissionStatus.Granted)
                {
                    StatusMessage = "Location permission denied.";
                    IsBusy = false;
                    return;
                }

                StatusMessage = "Starting GPS...";
                IsBusy = true;

                await _transitService.InitializeAsync();
                StartLocationUpdates();

                _isSharing = true;
                
                // Auto-update status and UI
                SelectedStatus = "Driving";
                ButtonText = "End Journey";
                ButtonColor = Colors.Red; // Red to Stop
                StatusMessage = $"Broadcasting: {SelectedStatus}";
            }
        }

        private void StartLocationUpdates()
        {
            if (Application.Current == null) return;

            _timer = Application.Current.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(5); // Send every 5 seconds
            _timer.Tick += async (s, e) => await SendLocation();
            _timer.Start();
        }

        private void StopLocationUpdates()
        {
            _timer?.Stop();
            _isSharing = false;
            IsBusy = false;
        }

        private async Task SendLocation()
        {
            try
            {
                // Get GPS Location
                // Use Medium accuracy for better indoor performance during testing
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                var location = await Geolocation.Default.GetLocationAsync(request);

                if (location != null)
                {
                    await _transitService.UpdateBusLocation(SelectedRoute, location.Latitude, location.Longitude, SelectedStatus);
                    // Do not overwrite StatusMessage if it shows passenger count, maybe show toast or debug?
                    // status message logic in OnSelectedScheduleItemChanged overwrites this.
                    // Let's keep status message simple for now.
                    // StatusMessage = $"Status: {SelectedStatus}\nLat: {location.Latitude:F4}, Lng: {location.Longitude:F4}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Driver] SendLocation Error: {ex.Message}");
                if (_isSharing)
                {
                    // Update UI to show something is wrong, but don't spam if it's transient
                    // StatusMessage = $"GPS Error: {ex.Message}"; 
                    // Better: keep status but maybe append error? Or just log.
                    // If it fails consistently, user needs to know.
                    MainThread.BeginInvokeOnMainThread(() => StatusMessage = $"Error: {ex.Message}");
                }
            }
        }
    }
}