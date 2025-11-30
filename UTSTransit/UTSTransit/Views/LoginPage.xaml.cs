using UTSTransit.ViewModels;
using UTSTransit.Services;

namespace UTSTransit.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
