using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UTSTransit.Services;

namespace UTSTransit.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly TransitService _transitService;

#pragma warning disable MVVMTK0045
        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _isBusy;
#pragma warning restore MVVMTK0045

        public LoginViewModel(TransitService transitService)
        {
            _transitService = transitService;
        }

        [RelayCommand]
        public async Task Login()
        {
            if (IsBusy) return;
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "Please enter email and password";
                return;
            }

            IsBusy = true;
            StatusMessage = "Logging in...";

            try
            {
                // 确保 Supabase 已初始化
                await _transitService.InitializeAsync();
                
                var success = await _transitService.LoginAsync(Email, Password);
                if (success)
                {
                    StatusMessage = "Login Successful!";
                    
                    // Check role and update UI
                    var role = _transitService.GetCurrentUserRole();
                    if (Shell.Current is AppShell appShell)
                    {
                        var isDriver = role == "driver";
                        appShell.SetDriverTabVisible(isDriver);
                        appShell.SetStudentTabsVisible(!isDriver); // Hide Schedule if Driver
                    }

                    // Navigate to main page
                    if (role == "driver")
                        await Shell.Current.GoToAsync("//DriverPage"); // Using ShellContent Route directly is often safer
                    else
                        await Shell.Current.GoToAsync("//MainTabs");
                }
                else
                {
                    StatusMessage = "Login failed, please check your email or password";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task GoToSignUp()
        {
            await Shell.Current.GoToAsync("SignUpPage");
        }

        [RelayCommand]
        public async Task GoToForgotPassword()
        {
            await Shell.Current.GoToAsync("ForgotPasswordPage");
        }
    }
}
