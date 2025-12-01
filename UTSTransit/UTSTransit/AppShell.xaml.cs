namespace UTSTransit
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(Views.SignUpPage), typeof(Views.SignUpPage));
            Routing.RegisterRoute(nameof(Views.ForgotPasswordPage), typeof(Views.ForgotPasswordPage));
        }
    }
}
