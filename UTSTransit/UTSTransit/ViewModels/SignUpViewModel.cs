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
        private bool _isDriver;

        [ObservableProperty]
        private string _studentId = string.Empty;

        [ObservableProperty]
        private string _icNumber = string.Empty;

        [ObservableProperty]
        private string _verificationCode = string.Empty;

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
                StatusMessage = "Please enter email and password.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                StatusMessage = "Passwords do not match.";
                return;
            }

            string role = "student";

            if (IsDriver)
            {
                if (string.IsNullOrWhiteSpace(VerificationCode) || VerificationCode.ToUpper() != "LIME6199")
                {
                    StatusMessage = "Invalid Verification Code. Please contact lime6199@gmail.com.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(IcNumber))
                {
                    StatusMessage = "Please enter your IC Number.";
                    return;
                }
                role = "driver";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(StudentId))
                {
                    StatusMessage = "Please enter your Student ID.";
                    return;
                }
            }

            IsBusy = true;
            StatusMessage = "Creating account...";

            try
            {
                await _transitService.InitializeAsync();
                var result = await _transitService.RegisterAsync(Email, Password, role, StudentId, IcNumber);

                if (result.IsSuccess)
                {
                    StatusMessage = "Registration successful! Please login.";
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    StatusMessage = $"Registration failed: {result.ErrorMessage}";
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
