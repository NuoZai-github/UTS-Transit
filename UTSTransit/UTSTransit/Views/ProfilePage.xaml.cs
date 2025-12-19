using UTSTransit.ViewModels;

namespace UTSTransit.Views;

public partial class ProfilePage : ContentPage
{
	public ProfilePage(ProfileViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Reload profile every time the page appears (fixes persistence issues on re-login)
        if (BindingContext is ProfileViewModel vm)
        {
            vm.LoadProfile();
        }
    }
}
