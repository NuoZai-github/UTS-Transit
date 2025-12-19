using Supabase.Realtime;
using UTSTransit.Models;
using System.Diagnostics;

namespace UTSTransit.Services
{
    public class TransitService
    {
        // Supabase URL 和 Key
        private const string SupabaseUrl = "https://dxinlpyicohuegachjdp.supabase.co";
        private const string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImR4aW5scHlpY29odWVnYWNoamRwIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjQwMzM5NjQsImV4cCI6MjA3OTYwOTk2NH0.sloSuXDB0Wtoumucox0Uc5NV2VhMmcGyHdcGf2gYgnc";

        private readonly Supabase.Client _client;

        public TransitService()
        {
            var options = new Supabase.SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false
            };
            _client = new Supabase.Client(SupabaseUrl, SupabaseKey, options);
        }

        public async Task InitializeAsync()
        {
            await _client.InitializeAsync();
        }

        public async Task ConnectRealtimeAsync()
        {
            try
            {
                if (_client.Realtime.Socket == null || !_client.Realtime.Socket.IsConnected)
                {
                    await _client.Realtime.ConnectAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Realtime Connect Failed: {ex.Message}");
            }
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
        public async Task<(bool IsSuccess, string ErrorMessage)> RegisterAsync(string email, string password, string role, string fullName, string studentId = null, string icNumber = null)
        {
            try
            {
                var data = new Dictionary<string, object>
                {
                    { "role", role },
                    { "full_name", fullName }
                };

                if (!string.IsNullOrEmpty(studentId)) data.Add("student_id", studentId);
                if (!string.IsNullOrEmpty(icNumber)) data.Add("ic_number", icNumber);

                var options = new Supabase.Gotrue.SignUpOptions
                {
                    Data = data
                };

                var session = await _client.Auth.SignUp(email, password, options);
                
                if (session == null)
                    return (false, "Unknown error: Session is null.");

                if (session.User != null && session.User.Identities != null && session.User.Identities.Count == 0)
                     return (false, "User already exists or email is invalid.");

                // Check if we actually have a session token. 
                // If Email Confirmation is OFF, we should have one.
                // If it's ON, we won't. But User assumes it's OFF for this Assignment.
                // If Session is missing but User exists, try explicit login!
                if (session.User != null && session.AccessToken == null)
                {
                    try 
                    {
                        // Try immediate login to allow uploading avatar
                        var loginSession = await _client.Auth.SignIn(email, password);
                        if (loginSession != null && loginSession.AccessToken != null)
                        {
                             try 
                             {
                                 var p = new Models.Profile 
                                 {
                                     Id = Guid.Parse(loginSession.User.Id),
                                     Email = email,
                                     Role = role,
                                     FullName = fullName,
                                     StudentId = studentId,
                                     IcNumber = icNumber,
                                     UpdatedAt = DateTime.UtcNow
                                 };
                                 await _client.From<Models.Profile>().Upsert(p);
                             }
                             catch (Exception manualEx) 
                             {
                                 Debug.WriteLine($"Manual Profile Upsert failed: {manualEx.Message}");
                             }

                             return (true, string.Empty);
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = ex.Message ?? "";
                        if (msg.Contains("Email not confirmed") || msg.ToLower().Contains("confirmed"))
                        {
                            return (true, "Account created! verification link sent to email.");
                        }
                    }
                    
                    // Failed to auto-login (maybe confirmation really is required?)
                    return (true, "Account created, but auto-login failed. Please verify email or login manually.");
                }

                if (session.User != null && !string.IsNullOrEmpty(session.AccessToken))
                {
                     try 
                     {
                         // Robustness: Explicitly save Profile data in case DB Trigger failed
                         var newProfile = new Models.Profile 
                         {
                             Id = Guid.Parse(session.User.Id),
                             Email = email,
                             Role = role,
                             FullName = fullName,
                             StudentId = studentId,
                             IcNumber = icNumber,
                             UpdatedAt = DateTime.UtcNow
                         };
                         
                         // Use Upsert to create or update
                         await _client.From<Models.Profile>().Upsert(newProfile);
                     }
                     catch (Exception pEx)
                     {
                         Debug.WriteLine($"Profile Manual Sync Warning: {pEx.Message}");
                         // Continue, do not fail registration
                     }
                }

                return (true, string.Empty);
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"Register Network Error: {httpEx.Message}");
                return (false, "Network error. Please check your internet connection.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Register Failed: {ex.Message}");
                return (false, ex.Message ?? "Unknown registration error.");
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

        public string? GetCurrentUserId()
        {
            return _client.Auth.CurrentUser?.Id;
        }

        public string GetCurrentUserEmail()
        {
            return _client.Auth.CurrentUser?.Email ?? "Guest";
        }

        public string GetCurrentUserRole()
        {
            if (_client.Auth.CurrentUser?.UserMetadata != null &&
                _client.Auth.CurrentUser.UserMetadata.ContainsKey("role"))
            {
                return _client.Auth.CurrentUser.UserMetadata["role"].ToString();
            }
            return "student"; 
        }

        public bool IsUserLoggedIn => _client.Auth.CurrentUser != null && !string.IsNullOrEmpty(_client.Auth.CurrentSession?.AccessToken);

        public List<Models.Announcement> GetAnnouncements()
        {
            return new List<Models.Announcement>
            {
                new Models.Announcement
                {
                    Title = "Heavy Rain Warning",
                    Content = "Due to heavy rain, buses on Route B may be delayed by 10-15 minutes.",
                    CreatedAt = DateTime.Now
                },
                new Models.Announcement
                {
                    Title = "Exam Week Schedule",
                    Content = "During exam week, we will be adding extra buses on Route A between 8 AM and 10 AM.",
                    CreatedAt = DateTime.Now.AddDays(-2)
                },
                new Models.Announcement
                {
                    Title = "App Maintenance",
                    Content = "The UTS Transit app will undergo maintenance this Sunday from 2 AM to 4 AM.",
                    CreatedAt = DateTime.Now.AddDays(-5)
                }
            };
        }

        // 司机：上传位置
        public async Task UpdateBusLocation(string routeName, double lat, double lng, string status)
        {
            await ConnectRealtimeAsync(); 

            var userId = GetCurrentUserId();

            var location = new BusLocation
            {
                DriverId = userId,
                RouteName = routeName,
                Latitude = lat,
                Longitude = lng,
                Status = status,
                LastUpdated = DateTime.UtcNow
            };

            try
            {
                // Upsert: 更新或插入
                await _client.From<BusLocation>().Upsert(location);
                Debug.WriteLine($"[Driver] Uploaded: {lat}, {lng}, {status}");
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
            await ConnectRealtimeAsync(); // Ensure connected

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

        // --- Admin Features ---

        // 1. User Management
        public async Task<List<Supabase.Gotrue.User>> GetUsersAsync()
        {
            // Note: Client-side listing of users is restricted. 
            // We will query the 'profiles' table instead which mirrors users.
            // Ensure you have a 'profiles' table with 'role' column.
            try
            {
                // This assumes you have a public.profiles table that is readable by admins
                // For this demo, we might need to mock or use a workaround if profiles table isn't fully set up
                // Let's try to fetch from a 'profiles' table model if we had one.
                // Since we don't have a Profile model yet, let's create a dynamic query or just return a placeholder for now
                // to avoid compilation errors.
                // Ideally: return await _client.From<Profile>().Get();
                return new List<Supabase.Gotrue.User>(); 
            }
            catch
            {
                return new List<Supabase.Gotrue.User>();
            }
        }

        // 2. Announcements (Real DB)
        public async Task<List<Announcement>> GetAnnouncementsAsync()
        {
            try
            {
                var response = await _client.From<Announcement>().Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending).Get();
                return response.Models;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fetch Announcements Error: {ex.Message}");
                return new List<Announcement>();
            }
        }

        public async Task CreateAnnouncementAsync(Announcement announcement)
        {
            await _client.From<Announcement>().Insert(announcement);
        }

        public async Task DeleteAnnouncementAsync(Guid id)
        {
            await _client.From<Announcement>().Where(x => x.Id == id).Delete();
        }

        // 3. Schedules (Real DB)
        public async Task<List<ScheduleItem>> GetSchedulesAsync()
        {
            try
            {
                var response = await _client.From<ScheduleItem>().Order("departure_time", Supabase.Postgrest.Constants.Ordering.Ascending).Get();
                return response.Models;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fetch Schedules Error: {ex.Message}");
                return new List<ScheduleItem>();
            }
        }

        // --- Booking Features ---

        public async Task<(bool IsSuccess, string Message)> BookSlotAsync(Guid scheduleId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId)) return (false, "Not logged in");

                var booking = new Models.Booking
                {
                    ScheduleId = scheduleId,
                    StudentId = Guid.Parse(userId),
                    BookingDate = DateTime.Today,
                    Status = "Booked"
                };

                await _client.From<Models.Booking>().Insert(booking);
                return (true, "Booking successful!");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("duplicate"))
                    return (false, "You have already booked this trip.");

                return (false, $"Booking failed: {ex.Message}");
            }
        }

        public async Task<List<Models.Booking>> GetStudentBookingsAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId)) return new List<Models.Booking>();

                var response = await _client.From<Models.Booking>()
                    .Where(x => x.StudentId == Guid.Parse(userId) && x.BookingDate == DateTime.Today)
                    .Get();
                return response.Models;
            }
            catch
            {
                return new List<Models.Booking>();
            }
        }

        public async Task<List<string>> GetBookedStudentsForScheduleAsync(Guid scheduleId)
        {
            try
            {
                // 1. Get all bookings for this schedule today
                var bookingsFn = await _client.From<Models.Booking>()
                    .Where(x => x.ScheduleId == scheduleId && x.BookingDate == DateTime.Today)
                    .Get();
                
                var bookings = bookingsFn.Models;
                if (!bookings.Any()) return new List<string>();

                // 2. Get student IDs
                var studentIds = bookings.Select(b => b.StudentId.ToString()).ToList();

                // 3. Fetch Profiles manually since Supabase-csharp join is tricky
                // Note: Where(x => list.Contains(x.Id)) might not work directly in all client versions, but let's try 'In' filter
                var profilesFn = await _client.From<Models.Profile>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.In, studentIds)
                    .Get();

                return profilesFn.Models.Select(p => p.StudentId ?? p.Email).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching student names: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task CreateScheduleAsync(ScheduleItem item)
        {
            await _client.From<ScheduleItem>().Insert(item);
        }

        public async Task DeleteScheduleAsync(Guid id)
        {
            await _client.From<ScheduleItem>().Where(x => x.Id == id).Delete();
        }

        // 4. Live Monitoring (Passengers)
        public async Task<List<TripPassenger>> GetTripPassengersAsync(int tripId)
        {
            try
            {
                var response = await _client.From<TripPassenger>().Where(x => x.TripId == tripId).Get();
                return response.Models;
            }
            catch
            {
                return new List<TripPassenger>();
            }
        }

        // --- Profile & Storage ---

        public async Task<string?> UploadProfileImageAsync(string userId, FileResult file)
        {
            try
            {
                // Fixed filename per user to replace old avatar
                var fileName = $"{userId}_avatar.jpg";
                using var stream = await file.OpenReadAsync();
                
                var storage = _client.Storage.From("avatars");
                
                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                // Upload with Upsert=true to overwrite existing file
                await storage.Upload(fileBytes, fileName, new Supabase.Storage.FileOptions { Upsert = true });

                return storage.GetPublicUrl(fileName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Upload Failed: {ex.Message}");
                // Rethrow to let UI know
                throw new Exception($"Storage error: {ex.Message}");
            }
        }

        public async Task UpdateProfileAvatarAsync(string userId, string avatarUrl)
        {
            try
            {
                // Use RPC (Remote Procedure Call) to bypass strict Table RLS.
                // The function 'update_avatar' runs with SECURITY DEFINER (Admin) privileges.
                var parameters = new Dictionary<string, object>
                {
                    { "p_avatar_url", avatarUrl }
                };

                await _client.Rpc("update_avatar", parameters);
                Debug.WriteLine($"[Profile] Updated Avatar via RPC: {avatarUrl}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RPC Update Failed: {ex.Message}");
                // Rethrow for UI
                throw new Exception($"Failed to update profile: {ex.Message}");
            }
        }

        public async Task<Models.Profile?> GetCurrentUserProfileAsync()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return null;

            try
            {
                // Attempt to fetch profile using RPC to bypass any RLS restriction on SELECT
                // The 'get_my_profile' function (if created) returns the user's profile ignoring RLS
                var response = await _client.Rpc("get_my_profile", null);
                
                if (response != null && !string.IsNullOrEmpty(response.Content))
                {
                    // The function returns a TABLE, which comes back as a JSON Array
                    var profiles = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Profile>>(response.Content);
                    if (profiles != null && profiles.Any())
                    {
                        Debug.WriteLine($"[Profile] Fetched via RPC: {profiles.First().Email}");
                        return profiles.First();
                    }
                }
                
                // If RPC return empty or failed (e.g. function not exists), try standard SELECT
                Debug.WriteLine("[Profile] RPC returned empty, trying standard Select...");
                var stdResponse = await _client.From<Models.Profile>()
                    .Where(x => x.Id == Guid.Parse(userId))
                    .Get();
                return stdResponse.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fetch Profile Error (RPC+Standard): {ex.Message}");
                // Last ditch effort: Try standard select if RPC crashed
                 try 
                {
                    var response = await _client.From<Models.Profile>()
                        .Where(x => x.Id == Guid.Parse(userId))
                        .Get();
                    return response.Models.FirstOrDefault();
                }
                catch { return null; }
            }
        }
    }
}