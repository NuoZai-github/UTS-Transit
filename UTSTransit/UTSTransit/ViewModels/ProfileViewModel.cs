using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UTSTransit.Services;

namespace UTSTransit.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly TransitService _transitService;

        [ObservableProperty]
        private string _fullName;

        [ObservableProperty]
        private string _role;
        
        [ObservableProperty]
        private string _userEmail;

        [ObservableProperty]
        private ImageSource _userAvatar = "user_placeholder.png";

        public ProfileViewModel(TransitService transitService)
        {
            _transitService = transitService;
            UserEmail = _transitService.GetCurrentUserEmail();
            // LoadProfile() is now called from OnAppearing in the Page code-behind
        }

        public async void LoadProfile()
        {
            try
            {
                var profile = await _transitService.GetCurrentUserProfileAsync();
                var userId = _transitService.GetCurrentUserId();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (profile != null)
                    {
                        FullName = profile.FullName ?? "No Name";
                        Role = profile.Role ?? "Student";
                        Console.WriteLine($"[ProfilePage] Loaded FullName: {FullName}, Role: {Role}");
                    }

                    // Force construct URL to bypass DB reading issues since filename is predictable
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var fixedUrl = $"https://dxinlpyicohuegachjdp.supabase.co/storage/v1/object/public/avatars/{userId}_avatar.jpg";
                        var uri = new Uri(fixedUrl + $"?t={DateTime.Now.Ticks}");
                        Console.WriteLine($"[ProfilePage] Force-setting Avatar Source: {uri}");
                        UserAvatar = ImageSource.FromUri(uri);
                    }
                    else
                    {
                        Console.WriteLine("[ProfilePage] No User ID found, keeping default avatar.");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadProfile Error: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task UploadAvatar()
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync();
                if (result != null)
                {
                    // Show loading or temp state if desired
                    var userId = _transitService.GetCurrentUserId();
                    if (string.IsNullOrEmpty(userId)) 
                    {
                        await Shell.Current.DisplayAlert("Error", "User ID not found (not logged in?).", "OK");
                        return;
                    }

                    // Pass the MediaPicker result directly. 
                    // Do NOT try to access FullPath and wrap it again, as FullPath can be null on Android.
                    var avatarUrl = await _transitService.UploadProfileImageAsync(userId, result);

                    if (!string.IsNullOrEmpty(avatarUrl))
                    {
                        await _transitService.UpdateProfileAvatarAsync(userId, avatarUrl);
                        
                        // Refresh display
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            UserAvatar = ImageSource.FromUri(new Uri(avatarUrl + $"?t={DateTime.Now.Ticks}"));
                        });
                        
                        await Shell.Current.DisplayAlert("Success", "Profile picture updated!", "OK");
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Failed to upload image.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Upload failed: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task Logout()
        {
            bool confirm = await Shell.Current.DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
            if (confirm)
            {
                await _transitService.LogoutAsync();
                
                // Reset UI state
                if (Shell.Current is AppShell appShell)
                {
                    appShell.SetDriverTabVisible(false);
                }

                // Navigate back to Login Page
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }
    }
}
