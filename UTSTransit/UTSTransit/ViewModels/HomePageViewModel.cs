using CommunityToolkit.Mvvm.ComponentModel;
using UTSTransit.Services;
using UTSTransit.Models;

namespace UTSTransit.ViewModels
{
    public partial class HomePageViewModel : ObservableObject
    {
        private readonly TransitService _transitService;

        [ObservableProperty]
        private string _statusText = "Waiting for updates...";

        [ObservableProperty]
        private string _lastUpdatedText = "";

        [ObservableProperty]
        private string _currentStatus = "Stopped"; // Driving, Resting, Arrived, Stopped

        public HomePageViewModel(TransitService transitService)
        {
            _transitService = transitService;
            Initialize();
        }

        private async void Initialize()
        {
            await _transitService.InitializeAsync();
            await _transitService.SubscribeToBusUpdates(OnBusUpdate);
        }

        private void OnBusUpdate(BusLocation bus)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CurrentStatus = bus.Status;
                LastUpdatedText = $"Last Updated: {bus.LastUpdated.ToLocalTime():T}";

                switch (bus.Status)
                {
                    case "Driving":
                        StatusText = $"Bus is Driving on {bus.RouteName}";
                        break;
                    case "Resting":
                        StatusText = "Bus is Resting 😴";
                        break;
                    case "Arrived at Campus":
                        StatusText = "Bus Arrived at Campus 🏫";
                        break;
                    case "Arrived at Hostel":
                        StatusText = "Bus Arrived at Hostel 🏠";
                        break;
                    case "Service Stopped":
                        StatusText = "Service Stopped for Today 🛑";
                        break;
                    default:
                        StatusText = $"Status: {bus.Status}";
                        break;
                }
            });
        }
    }
}
