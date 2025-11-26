using Supabase.Realtime;
using UTSTransit.Models;
using System.Diagnostics;

namespace UTSTransit.Services
{
    public class TransitService
    {
        // 请替换为你自己的 Supabase URL 和 Key
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

        public string GetCurrentUserId()
        {
            // 简化版：如果没有登录，给一个模拟ID
            return _client.Auth.CurrentUser?.Id ?? "simulated-driver-id-001";
        }

        // 司机：上传位置
        public async Task UpdateBusLocation(string routeName, double lat, double lng)
        {
            var userId = GetCurrentUserId();

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
                // Upsert: 更新或插入
                await _client.From<BusLocation>().Upsert(location);
                Debug.WriteLine($"[Driver] Uploaded: {lat}, {lng}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Driver] Error: {ex.Message}");
            }
        }

        // 司机：停止行程
        public async Task StopSharing()
        {
            var userId = GetCurrentUserId();
            await _client.From<BusLocation>().Where(x => x.DriverId == userId).Delete();
        }

        // 学生：订阅位置
        public async Task SubscribeToBusUpdates(Action<BusLocation> onUpdate)
        {
            // 使用完整枚举路径
            await _client.From<BusLocation>()
                .On(Supabase.Realtime.PostgresChanges.EventType.Update, (sender, change) =>
                {
                    var updatedBus = change.Model<BusLocation>();
                    onUpdate?.Invoke(updatedBus);
                })
                .On(Supabase.Realtime.PostgresChanges.EventType.Insert, (sender, change) =>
                {
                    var newBus = change.Model<BusLocation>();
                    onUpdate?.Invoke(newBus);
                })
                .Subscribe();
        }
    }
}