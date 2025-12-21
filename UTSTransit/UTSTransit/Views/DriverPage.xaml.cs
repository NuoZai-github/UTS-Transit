using UTSTransit.ViewModels;
using UTSTransit.Services;

namespace UTSTransit.Views;

public partial class DriverPage : ContentPage
{
    public DriverPage(TransitService service)
    {
        InitializeComponent();
        BindingContext = new DriverViewModel(service);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CheckAndRequestLocationPermission();
    }

    private async Task CheckAndRequestLocationPermission()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Permission request failed: {ex.Message}");
        }
    }
}