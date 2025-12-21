using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using UTSTransit.Models;
using UTSTransit.Services;
using Microsoft.Maui.Devices.Sensors;
// using Microsoft.Maui.Controls.Maps; // Removed
// using Microsoft.Maui.Maps; // Removed

namespace UTSTransit.ViewModels
{
    public partial class MapViewModel : ObservableObject
    {
        private readonly TransitService _transitService;
        private readonly RouteService _routeService;

        // 存放地图上的大头针
        public ObservableCollection<BusPinModel> BusPins { get; } = new();

        // 存放路线信息
        public List<RouteInfo> Routes { get; private set; }

        public MapViewModel(TransitService transitService, RouteService routeService)
        {
            _transitService = transitService;
            _routeService = routeService;
            Routes = _routeService.GetRoutes();
            InitializeRealtime();
        }

        private async void InitializeRealtime()
        {
            try
            {
                await _transitService.InitializeAsync();

                // 1. Initial Fetch
                var initialBuses = await _transitService.GetCurrentBusLocationsAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (initialBuses.Count > 0)
                    {
                        foreach (var bus in initialBuses) UpdateMapPin(bus);
                    }
                    else
                    {
                        IsBusActive = false;
                    }
                });

                // 2. Realtime Subscription
                await _transitService.SubscribeToBusUpdates((bus) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        UpdateMapPin(bus);
                    });
                });

                // 3. Polling Fallback (Every 5 seconds)
                if (Application.Current != null)
                {
                    var timer = Application.Current.Dispatcher.CreateTimer();
                    timer.Interval = TimeSpan.FromSeconds(5);
                    timer.Tick += async (s, e) =>
                    {
                        var buses = await _transitService.GetCurrentBusLocationsAsync();
                        if (buses.Count == 0)
                        {
                            BusPins.Clear();
                            IsBusActive = false;
                        }
                        else
                        {
                            // Update existing
                            foreach (var bus in buses) UpdateMapPin(bus);
                            
                            // Remove stalled/deleted pins (simple logic: clear if not in current list?)
                            // For now, just keeping it simple. 
                            // Polling ensures that at least we get updates if Realtime fails.
                        }
                    };
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing realtime: {ex.Message}");
            }
        }

        [ObservableProperty]
        private double _busProgress; // 0.0 (Hostel) to 1.0 (Campus)

        [ObservableProperty]
        private double _busRotation; // 0 = Face Right, 180 = Face Left

        [ObservableProperty]
        private bool _isBusActive;

        private void UpdateMapPin(BusLocation bus)
        {
            var existingPin = BusPins.FirstOrDefault(p => p.Label == bus.RouteName);

            if (existingPin != null)
            {
                existingPin.Latitude = bus.Latitude;
                existingPin.Longitude = bus.Longitude;
                existingPin.Address = $"Updated: {bus.LastUpdated.ToLocalTime():T}";
                
                var index = BusPins.IndexOf(existingPin);
                BusPins[index] = existingPin; 
            }
            else
            {
                var newPin = new BusPinModel
                {
                    Label = bus.RouteName,
                    Address = $"Updated: {bus.LastUpdated.ToLocalTime():T}",
                    Latitude = bus.Latitude,
                    Longitude = bus.Longitude
                };
                BusPins.Add(newPin);
            }

            // --- Update Status Bar Logic ---
            IsBusActive = true;

            // Fixed Locations
            var hostel = new Microsoft.Maui.Devices.Sensors.Location(2.3420, 111.8318);
            var campus = new Microsoft.Maui.Devices.Sensors.Location(2.3417, 111.8442);
            var busLoc = new Microsoft.Maui.Devices.Sensors.Location(bus.Latitude, bus.Longitude);

            double totalDist = Microsoft.Maui.Devices.Sensors.Location.CalculateDistance(hostel, campus, Microsoft.Maui.Devices.Sensors.DistanceUnits.Kilometers);
            double distFromHostel = Microsoft.Maui.Devices.Sensors.Location.CalculateDistance(hostel, busLoc, Microsoft.Maui.Devices.Sensors.DistanceUnits.Kilometers);
            
            // Calculate Progress (0 = Hostel, 1 = Campus)
            // Note: TotalDist is straight line, but simple ratio works for status bar approximation
            // Ideally project point onto line, but distance ratio is sufficient for UI
            double rawProgress = 0;
            if (totalDist > 0)
                rawProgress = distFromHostel / totalDist;
            
            BusProgress = Math.Clamp(rawProgress, 0, 1);

            // Determine Rotation based on Route Name
            // Route A: Dorm -> Campus (Left to Right) -> Face Right (0)
            // Route B: Campus -> Hostel (Right to Left) -> Face Left (180)
            if (bus.RouteName != null && (bus.RouteName.Contains("Route B") || bus.RouteName.Contains("Hostel")))
            {
                 BusRotation = 180; 
            }
            else
            {
                 BusRotation = 0;
            }
        }
    }
}