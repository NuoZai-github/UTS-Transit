using UTSTransit.ViewModels;

namespace UTSTransit.Views;

public partial class ProfilePage : ContentPage
{
	public ProfilePage(ProfileViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
