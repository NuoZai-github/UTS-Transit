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
}