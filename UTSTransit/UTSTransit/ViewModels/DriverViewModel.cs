using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UTSTransit.Services;

namespace UTSTransit.ViewModels
{
    public partial class DriverViewModel : ObservableObject
    {
        private readonly TransitService _transitService;
        private IDispatcherTimer _timer;
        private bool _isSharing;

        [ObservableProperty]
        private string statusMessage = "等待开始行程...";

        [ObservableProperty]
        private string selectedRoute = "Route A (Dorm -> Campus)";

        [ObservableProperty]
        private bool isBusy;

        public DriverViewModel(TransitService transitService)
        {
            _transitService = transitService;
        }

        [RelayCommand]
        public async Task ToggleSharing()
        {
            if (_isSharing)
            {
                StopLocationUpdates();
                StatusMessage = "行程已结束，位置共享停止。";
                await _transitService.StopSharing();
            }
            else
            {
                StatusMessage = "正在获取 GPS...";
                IsBusy = true;

                StartLocationUpdates();
                _isSharing = true;
                StatusMessage = "行程进行中 - 正在广播位置";
            }
        }

        private void StartLocationUpdates()
        {
            _timer = Application.Current.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += async (s, e) => await SendLocation();
            _timer.Start();
        }

        private void StopLocationUpdates()
        {
            _timer?.Stop();
            _isSharing = false;
            IsBusy = false;
        }

        private async Task SendLocation()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                var location = await Geolocation.Default.GetLocationAsync(request);

                if (location != null)
                {
                    await _transitService.UpdateBusLocation(SelectedRoute, location.Latitude, location.Longitude);
                    StatusMessage = $"位置已发送: {DateTime.Now:T}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"GPS 错误: {ex.Message}";
            }
        }
    }
}