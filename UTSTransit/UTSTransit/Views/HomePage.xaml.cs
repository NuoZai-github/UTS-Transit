using UTSTransit.ViewModels;

namespace UTSTransit.Views
{
    public partial class HomePage : ContentPage
    {
        private bool _isAnimating;

        public HomePage(HomePageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private async void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HomePageViewModel.CurrentStatus))
            {
                var vm = BindingContext as HomePageViewModel;
                await UpdateAnimation(vm?.CurrentStatus);
            }
        }

        private async Task UpdateAnimation(string? status)
        {
            // Reset
            _isAnimating = false;
            BusImage.CancelAnimations();
            StopSign.IsVisible = false;
            ZzzText.IsVisible = false;
            BusImage.Rotation = 0;
            BusImage.TranslationX = 0;

            if (string.IsNullOrEmpty(status)) return;

            switch (status)
            {
                case "Driving":
                    _isAnimating = true;
                    await AnimateDriving();
                    break;
                case "Resting":
                    BusImage.TranslationX = (this.Width / 2) - 50; // Center
                    ZzzText.IsVisible = true;
                    await AnimateResting();
                    break;
                case "Arrived at Campus":
                case "Arrived at Hostel":
                    BusImage.TranslationX = (this.Width / 2) - 50; // Center
                    StopSign.IsVisible = true;
                    break;
                case "Service Stopped":
                    BusImage.TranslationX = 0;
                    BusImage.Opacity = 0.5;
                    break;
            }
        }

        private async Task AnimateDriving()
        {
            while (_isAnimating)
            {
                BusImage.TranslationX = -100;
                await BusImage.TranslateTo(this.Width, 0, 3000, Easing.Linear);
                if (!_isAnimating) break;
            }
        }

        private async Task AnimateResting()
        {
            while (ZzzText.IsVisible)
            {
                await ZzzText.ScaleTo(1.2, 1000);
                await ZzzText.ScaleTo(1.0, 1000);
            }
        }
    }
}
