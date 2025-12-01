using UTSTransit.ViewModels;

namespace UTSTransit.Views;

public partial class TimetablePage : ContentPage
{
	public TimetablePage(TimetableViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
