using UTSTransit.ViewModels;

namespace UTSTransit.Views
{
    public partial class HomePage : ContentPage
    {
        public HomePage(HomePageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is HomePageViewModel vm)
            {
                await vm.LoadProfileData();
            }
        }
    }
}
