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

        // 身份验证：登录
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

        // 身份验证：注册
        public async Task<bool> RegisterAsync(string email, string password, string role)
        {
            try
            {
                var options = new Supabase.Gotrue.SessionOptions
                {
                    Data = new Dictionary<string, object>
                    {
                        { "role", role }
                    }
                };

                var session = await _client.Auth.SignUp(email, password, options);
                return session != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Register Failed: {ex.Message}");
                return false;
            }
        }

        // 身份验证：注销
        public async Task LogoutAsync()
        {
            await _client.Auth.SignOut();
        }

        // 身份验证：重置密码
        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                await _client.Auth.ResetPasswordForEmail(email);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Reset Password Failed: {ex.Message}");
                return false;
            }
        }

        public string GetCurrentUserId()
        {
            // 如果没有登录，给一个模拟ID (仅用于测试)
            return _client.Auth.CurrentUser?.Id ?? "a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11";
        }

        public string GetCurrentUserEmail()
        {
            return _client.Auth.CurrentUser?.Email ?? "Guest User";
        }

        public string GetCurrentUserRole()
        {
            if (_client.Auth.CurrentUser?.UserMetadata != null &&
                _client.Auth.CurrentUser.UserMetadata.ContainsKey("role"))
            {
                return _client.Auth.CurrentUser.UserMetadata["role"].ToString();
            }
            return "student"; // Default to student
        }

        public bool IsUserLoggedIn => _client.Auth.CurrentUser != null;

        public List<Models.Announcement> GetAnnouncements()
        {
            return new List<Models.Announcement>
            {
                new Models.Announcement
                {
                    Title = "Heavy Rain Warning",
                    Content = "Due to heavy rain, buses on Route B may be delayed by 10-15 minutes.",
                    Date = DateTime.Now
                },
                new Models.Announcement
                {
                    Title = "Exam Week Schedule",
                    Content = "During exam week, we will be adding extra buses on Route A between 8 AM and 10 AM.",
                    Date = DateTime.Now.AddDays(-2)
                },
                new Models.Announcement
                {
                    Title = "App Maintenance",
                    Content = "The UTS Transit app will undergo maintenance this Sunday from 2 AM to 4 AM.",
                    Date = DateTime.Now.AddDays(-5)
                }
            };
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

        public async Task SubscribeToBusUpdates(Action<BusLocation> onUpdate)
        {
            // 使用 ListenType.All 订阅所有事件，然后在回调中过滤
            var channel = await _client.From<BusLocation>()
                .On(Supabase.Realtime.PostgresChanges.PostgresChangesOptions.ListenType.All, (sender, change) =>
                {
                    var bus = change.Model<BusLocation>();
                    // 如果是 Delete 事件，Model<T> 可能返回部分空值的对象，或者我们需要通过其他方式判断
                    // 这里简单通过检查关键属性是否为空来过滤
                    if (bus == null || string.IsNullOrEmpty(bus.RouteName)) return;

                    onUpdate?.Invoke(bus);
                });

            await channel.Subscribe();
        }
    }
}