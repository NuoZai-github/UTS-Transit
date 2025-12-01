using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using UTSTransit.Models;
using UTSTransit.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace UTSTransit.ViewModels
{
    public partial class MapViewModel : ObservableObject
    {
        private readonly TransitService _transitService;

        // 存放地图上的大头针
        public ObservableCollection<Pin> BusPins { get; } = new();

        public MapViewModel(TransitService transitService)
        {
            _transitService = transitService;
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
                existingPin.Location = new Location(bus.Latitude, bus.Longitude);
                existingPin.Address = $"Updated: {bus.LastUpdated.ToLocalTime():T}";
            }
            else
            {
                var newPin = new Pin
                {
                    Label = bus.RouteName,
                    Address = $"Updated: {bus.LastUpdated.ToLocalTime():T}",
                    Type = PinType.Place,
                    Location = new Location(bus.Latitude, bus.Longitude)
                };
                BusPins.Add(newPin);
            }
        }
    }
}