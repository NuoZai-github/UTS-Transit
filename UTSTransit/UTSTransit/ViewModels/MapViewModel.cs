using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using UTSTransit.Models;
using UTSTransit.Services;
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

                // 订阅更新
                await _transitService.SubscribeToBusUpdates((bus) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        UpdateMapPin(bus);
                    });
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing realtime: {ex.Message}");
            }
        }

        private void UpdateMapPin(BusLocation bus)
        {
            var existingPin = BusPins.FirstOrDefault(p => p.Label == bus.RouteName);

            if (existingPin != null)
            {
                existingPin.Latitude = bus.Latitude;
                existingPin.Longitude = bus.Longitude;
                existingPin.Address = $"Updated: {bus.LastUpdated.ToLocalTime():T}";
                
                // Trigger update manually if needed, or replace item to trigger CollectionChanged
                // For simplicity, let's replace it or rely on property change if BusPinModel implemented INotifyPropertyChanged
                // Since it doesn't, let's remove and add (simplest for now to trigger Map update)
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
        }
    }
}