using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UTSTransit.Services;

namespace UTSTransit.ViewModels
{
    public partial class ForgotPasswordViewModel : ObservableObject
    {
        private readonly TransitService _transitService;

#pragma warning disable MVVMTK0045
        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _isBusy;
#pragma warning restore MVVMTK0045

        public ForgotPasswordViewModel(TransitService transitService)
        {
            _transitService = transitService;
        }

        [RelayCommand]
        public async Task ResetPassword()
        {
            if (IsBusy) return;
            if (string.IsNullOrWhiteSpace(Email))
            {
                StatusMessage = "Please enter email";
                return;
            }

            IsBusy = true;
            StatusMessage = "Sending reset email...";

            try
            {
                await _transitService.InitializeAsync();

                var success = await _transitService.ResetPasswordAsync(Email);
                if (success)
                {
                    StatusMessage = "Reset email sent, please check your inbox.";
                }
                else
                {
                    StatusMessage = "Failed to send, please check if email is correct";
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
