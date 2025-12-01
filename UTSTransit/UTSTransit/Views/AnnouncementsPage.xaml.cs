using UTSTransit.ViewModels;

namespace UTSTransit.Views;

public partial class AnnouncementsPage : ContentPage
{
	public AnnouncementsPage(AnnouncementsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
