using UTSTransit.ViewModels;

namespace UTSTransit.Views;

public partial class AdminDashboardPage : ContentPage
{
	public AdminDashboardPage(AdminDashboardViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
