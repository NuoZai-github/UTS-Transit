using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UTSTransit.Services;

namespace UTSTransit.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly TransitService _transitService;

        [ObservableProperty]
        private string _userEmail;

        public ProfileViewModel(TransitService transitService)
        {
            _transitService = transitService;
            UserEmail = _transitService.GetCurrentUserEmail();
        }

        [RelayCommand]
        public async Task Logout()
        {
            bool confirm = await Shell.Current.DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
            if (confirm)
            {
                await _transitService.LogoutAsync();
                // Navigate back to Login Page
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }
    }
}
