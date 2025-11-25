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

        public ObservableCollection<Pin> BusPins { get; } = new();

        public MapViewModel(TransitService transitService)
        {
            _transitService = transitService;
            InitializeRealtime();
        }

        private async void InitializeRealtime()
        {
            await _transitService.InitializeAsync();

            await _transitService.SubscribeToBusUpdates((bus) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateMapPin(bus);
                });
            });
        }

        private void UpdateMapPin(BusLocation bus)
        {
            var existingPin = BusPins.FirstOrDefault(p => p.Label.Contains(bus.RouteName));

            if (existingPin != null)
            {
                existingPin.Location = new Location(bus.Latitude, bus.Longitude);
            }
            else
            {
                var newPin = new Pin
                {
                    Label = bus.RouteName,
                    Address = $"最后更新: {bus.LastUpdated.ToLocalTime():T}",
                    Type = PinType.Place,
                    Location = new Location(bus.Latitude, bus.Longitude)
                };
                BusPins.Add(newPin);
            }
        }
    }
}