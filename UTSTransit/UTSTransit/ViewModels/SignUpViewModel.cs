using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UTSTransit.Services;

namespace UTSTransit.ViewModels
{
    public partial class SignUpViewModel : ObservableObject
    {
        private readonly TransitService _transitService;

#pragma warning disable MVVMTK0045
        public SignUpViewModel(TransitService transitService)
        {
            _transitService = transitService;
        }

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
        private string _fullName = string.Empty;

        [ObservableProperty]
        private bool _isBusy;
#pragma warning restore MVVMTK0045

        [RelayCommand]
        public async Task Register()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "Please enter email and password.";
                return;
            }

            if (string.IsNullOrWhiteSpace(FullName))
            {
                StatusMessage = "Please enter your Real Full Name.";
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
                if (_transitService == null)
                {
                    StatusMessage = "Error: Service unavailable.";
                    return;
                }

                StatusMessage = "Connecting...";
                await _transitService.InitializeAsync();
                
                StatusMessage = "Creating account...";
                var result = await _transitService.RegisterAsync(
                    Email ?? "", 
                    Password ?? "", 
                    role ?? "student", 
                    FullName ?? "", 
                    StudentId, 
                    IcNumber);

                if (result.IsSuccess)
                {
                    // Case 1: Email Confirmation Required (Common for this assignment setup)
                    if (!string.IsNullOrEmpty(result.ErrorMessage) && 
                       (result.ErrorMessage.Contains("verification") || result.ErrorMessage.ToLower().Contains("confirmed")))
                    {
                        await Shell.Current.DisplayAlert("Registration Successful", 
                            "Please check your email to verify your account before logging in.", "OK");
                        await Shell.Current.GoToAsync(".."); // Return to Login Page
                        return;
                    }

                    // Case 2: Auto-login worked
                    await Shell.Current.DisplayAlert("Success", "Account created successfully.", "OK");
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
