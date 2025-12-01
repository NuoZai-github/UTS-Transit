using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UTSTransit.Services;

namespace UTSTransit.ViewModels
{
    public partial class SignUpViewModel : ObservableObject
    {
        private readonly TransitService _transitService;

#pragma warning disable MVVMTK0045
        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _isBusy;
#pragma warning restore MVVMTK0045

        public SignUpViewModel(TransitService transitService)
        {
            _transitService = transitService;
        }

        [RelayCommand]
        public async Task Register()
        {
            if (IsBusy) return;
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "Please enter email and password";
                return;
            }

            if (Password != ConfirmPassword)
            {
                StatusMessage = "Passwords do not match";
                return;
            }

            IsBusy = true;
            StatusMessage = "Registering...";

            try
            {
                await _transitService.InitializeAsync();

                var success = await _transitService.RegisterAsync(Email, Password);
                if (success)
                {
                    StatusMessage = "Registration successful! Please check your email or login directly.";
                    // 注册成功后返回登录页
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    StatusMessage = "Registration failed, please try again later";
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
        public async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
