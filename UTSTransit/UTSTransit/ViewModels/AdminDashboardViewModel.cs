using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UTSTransit.Services;
using UTSTransit.Models;
using System.Collections.ObjectModel;

namespace UTSTransit.ViewModels
{
    public partial class AdminDashboardViewModel : ObservableObject
    {
        private readonly TransitService _transitService;

        [ObservableProperty]
        private int _studentCount;

        [ObservableProperty]
        private int _driverCount;

        [ObservableProperty]
        private int _activeBusCount;

        public ObservableCollection<BusLocation> ActiveBuses { get; } = new();

        public AdminDashboardViewModel(TransitService transitService)
        {
            _transitService = transitService;
            LoadStatsCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadStats()
        {
            // Mock stats for now until we have full Profiles table access
            StudentCount = 120; 
            DriverCount = 5;
            
            // Get active buses
             _transitService.SubscribeToBusUpdates(bus =>
            {
                // Update local list
                var existing = ActiveBuses.FirstOrDefault(b => b.DriverId == bus.DriverId);
                if (existing != null)
                {
                    existing.Latitude = bus.Latitude;
                    existing.Longitude = bus.Longitude;
                    existing.Status = bus.Status;
                }
                else
                {
                    ActiveBuses.Add(bus);
                }
                ActiveBusCount = ActiveBuses.Count;
            });
        }
    }
}
