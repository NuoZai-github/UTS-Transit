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
        private string _selectedRoute = "Route A (Hostel -> Campus)";

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
        public ObservableCollection<string> PassengerList { get; } = new();

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
            foreach (var item in items) Schedules.Add(item);
        }

        async partial void OnSelectedScheduleItemChanged(Models.ScheduleItem value)
        {
            if (value == null) return;
            IsBusy = true;
            SelectedRoute = value.RouteName; // Sync route name

            // Fetch students
            var students = await _transitService.GetBookedStudentsForScheduleAsync(value.Id);
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
                StopLocationUpdates();
                StatusMessage = "Trip Ended";
                await _transitService.StopSharing();
            }
            else
            {
                StatusMessage = "Starting GPS...";
                IsBusy = true;

                await _transitService.InitializeAsync();
                StartLocationUpdates();

                _isSharing = true;
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
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
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
                // StatusMessage = $"GPS Error: {ex.Message}";
            }
        }
    }
}