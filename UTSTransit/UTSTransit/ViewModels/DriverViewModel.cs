using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        private string _statusMessage = "Waiting to start trip...";

        [ObservableProperty]
        private string _selectedRoute = "Route A (Dorm -> Campus)";

        [ObservableProperty]
        private bool _isBusy;
#pragma warning restore MVVMTK0045

        public DriverViewModel(TransitService transitService)
        {
            _transitService = transitService;
            CheckDriverRole();
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
                StatusMessage = "Broadcasting location...";
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
                    await _transitService.UpdateBusLocation(SelectedRoute, location.Latitude, location.Longitude);
                    StatusMessage = $"Location Updated: {DateTime.Now:T}\nLat: {location.Latitude:F4}, Lng: {location.Longitude:F4}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"GPS Error: {ex.Message}";
            }
        }
    }
}