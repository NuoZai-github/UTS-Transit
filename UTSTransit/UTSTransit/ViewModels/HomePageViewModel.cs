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
        private string _greeting = "Good Morning,";

        [ObservableProperty]
        private string _lastUpdatedText = "";

        [ObservableProperty]
        private string _currentStatus = "Stopped"; 
        
        [ObservableProperty]
        private string _userName = "Student"; // Default name

        [ObservableProperty]
        private ImageSource? _userAvatar;

        [ObservableProperty]
        private double _busProgress;

        [ObservableProperty]
        private bool _isBusVisible;

        // Approximate Coordinates for Progress Calculation (Adjust as needed)
        private const double HostelLat = 2.330; 
        private const double HostelLng = 111.820; 
        private const double CampusLat = 2.343; 
        private const double CampusLng = 111.833; 

        [ObservableProperty]
        private bool _hasAvatar;

        public HomePageViewModel(TransitService transitService)
        {
            _transitService = transitService;
        }

        public async Task LoadProfileData()
        {
            UpdateGreeting();
            await _transitService.InitializeAsync();

            var profile = await _transitService.GetCurrentUserProfileAsync();
            if (profile != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (!string.IsNullOrEmpty(profile.FullName))
                    {
                        UserName = profile.FullName;
                    }
                    else if (!string.IsNullOrEmpty(profile.Email))
                    {
                        UserName = profile.Email.Split('@')[0];
                    }
                    
                    if (!string.IsNullOrEmpty(profile.AvatarUrl))
                    {
                        Console.WriteLine($"[HomePage] Found Avatar: {profile.AvatarUrl}");
                        HasAvatar = true;
                        UserAvatar = ImageSource.FromUri(new Uri(profile.AvatarUrl + $"?t={DateTime.Now.Ticks}"));
                    }
                    else
                    {
                         Console.WriteLine($"[HomePage] No Avatar URL found, showing placeholder.");
                         HasAvatar = true; 
                         UserAvatar = "user_placeholder.png"; 
                    }
                });
            }

            await _transitService.SubscribeToBusUpdates(OnBusUpdate);
        }

        private void UpdateGreeting()
        {
            var hour = DateTime.Now.Hour;
            if (hour >= 5 && hour < 12)
            {
                Greeting = "Good Morning,";
            }
            else if (hour >= 12 && hour < 18)
            {
                Greeting = "Good Afternoon,";
            }
            else
            {
                Greeting = "Good Evening,";
            }
        }

        private void OnBusUpdate(BusLocation bus)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CurrentStatus = bus.Status;
                LastUpdatedText = $"Last Updated: {bus.LastUpdated.ToLocalTime():T}";

                // Update Status Text
                switch (bus.Status)
                {
                    case "Driving":
                        StatusText = $"Bus is Driving on {bus.RouteName}";
                        IsBusVisible = true;
                        break;
                    case "Resting":
                        StatusText = "Bus is Resting 😴";
                        IsBusVisible = false;
                        BusProgress = 0.0;
                        break;
                    case "Arrived at Campus":
                        StatusText = "Bus Arrived at Campus 🏫";
                        IsBusVisible = true;
                        BusProgress = 1.0;
                        break;
                    case "Arrived at Hostel":
                        StatusText = "Bus Arrived at Hostel 🏠";
                        IsBusVisible = true;
                        BusProgress = 0.0;
                        break;
                    case "Service Stopped":
                        StatusText = "Service Stopped for Today 🛑";
                        IsBusVisible = false;
                        break;
                    default:
                        StatusText = $"Status: {bus.Status}";
                        IsBusVisible = true;
                        break;
                }

                // Update Progress if Driving
                if (bus.Status == "Driving" && bus.Latitude != 0 && bus.Longitude != 0)
                {
                   double totalDist = GetDistance(HostelLat, HostelLng, CampusLat, CampusLng);
                   double distFromHostel = GetDistance(HostelLat, HostelLng, bus.Latitude, bus.Longitude);
                   
                   // Clamp 0-1
                   var p = distFromHostel / totalDist;
                   if (p < 0) p = 0;
                   if (p > 1) p = 1;
                   
                   // Direction Adjustment? 
                   // If Route is "Campus -> Hostel", progress should technically invert visuals if we want strict geographical mapping,
                   // but usually users prefer "Left(Start) -> Right(End)".
                   // For now, let's keep Left=Hostel, Right=Campus.
                   // So if going Campus->Hostel, progress goes 1.0 -> 0.0. 
                   // This works naturally with the logic above (distFromHostel decreases).
                   
                   BusProgress = p;
                }
            });
        }

        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var r = 6371; // Radius of the earth in km
            var dLat = Deg2Rad(lat2 - lat1);
            var dLon = Deg2Rad(lon2 - lon1);
            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return r * c; // Distance in km
        }

        private double Deg2Rad(double deg)
        {
            return deg * (Math.PI / 180);
        }
    }
}
