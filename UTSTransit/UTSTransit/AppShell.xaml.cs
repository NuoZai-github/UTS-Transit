namespace UTSTransit
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("SignUpPage", typeof(Views.SignUpPage));
            Routing.RegisterRoute("ForgotPasswordPage", typeof(Views.ForgotPasswordPage));
        }

        public void SetDriverTabVisible(bool isVisible)
        {
            if (DriverTab != null)
            {
                DriverTab.IsVisible = isVisible;
            }
        }
    }
}
