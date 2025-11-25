using Supabase.Realtime;
using UTSTransit.Models;

namespace UTSTransit.Services
{
    public class TransitService
    {
        private const string SupabaseUrl = "YOUR_SUPABASE_URL";
        private const string SupabaseKey = "YOUR_SUPABASE_ANON_KEY";

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
            catch
            {
                return false;
            }
        }

        public async Task UpdateBusLocation(string routeName, double lat, double lng)
        {
            var userId = _client.Auth.CurrentUser?.Id;
            if (string.IsNullOrEmpty(userId)) return;

            var location = new BusLocation
            {
                DriverId = userId,
                RouteName = routeName,
                Latitude = lat,
                Longitude = lng,
                LastUpdated = DateTime.UtcNow
            };

            await _client.From<BusLocation>().Upsert(location);
        }

        public async Task SubscribeToBusUpdates(Action<BusLocation> onUpdate)
        {
            await _client.From<BusLocation>()
                .On(PostgresChanges.Update, (sender, change) =>
                {
                    var updatedBus = change.Model<BusLocation>();
                    onUpdate?.Invoke(updatedBus);
                });
        }
    }
}
