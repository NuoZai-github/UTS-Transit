using Supabase.Realtime;
using UTSTransit.Models;
using System.Diagnostics;

namespace UTSTransit.Services
{
    public class TransitService
    {
        private const string SupabaseUrl = "https://dxinlpyicohuegachjdp.supabase.co";
        private const string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImR4aW5scHlpY29odWVnYWNoamRwIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjQwMzM5NjQsImV4cCI6MjA3OTYwOTk2NH0.sloSuXDB0Wtoumucox0Uc5NV2VhMmcGyHdcGf2gYgnc";

        private readonly Supabase.Client _client;

        public TransitService()
        {
            var options = new Supabase.SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };
            _client = new Supabase.Client(SupabaseUrl, SupabaseKey, options);
        }

        public async Task InitializeAsync()
        {
            await _client.InitializeAsync();
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var session = await _client.Auth.SignIn(email, password);
                return session != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login Failed: {ex.Message}");
                return false;
            }
        }

        public string GetCurrentUserId()
        {
            return _client.Auth.CurrentUser?.Id;
        }

        public async Task UpdateBusLocation(string routeName, double lat, double lng)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return;

            var location = new BusLocation
            {
                DriverId = userId,
                RouteName = routeName,
                Latitude = lat,
                Longitude = lng,
                LastUpdated = DateTime.UtcNow
            };

            try
            {
                await _client.From<BusLocation>().Upsert(location);
                Debug.WriteLine($"[Driver] Location sent: {lat}, {lng}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Driver] Error sending location: {ex.Message}");
            }
        }

        public async Task StopSharing()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return;

            await _client.From<BusLocation>().Where(x => x.DriverId == userId).Delete();
        }

        public async Task SubscribeToBusUpdates(Action<BusLocation> onUpdate)
        {
            await _client.From<BusLocation>()
                .On(PostgresChanges.Update, (sender, change) =>
                {
                    var updatedBus = change.Model<BusLocation>();
                    onUpdate?.Invoke(updatedBus);
                })
                .On(PostgresChanges.Insert, (sender, change) =>
                {
                    var newBus = change.Model<BusLocation>();
                    onUpdate?.Invoke(newBus);
                })
                .Subscribe();
        }
    }
}