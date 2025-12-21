namespace UTSTransit
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("SignUpPage", typeof(Views.SignUpPage));
            Routing.RegisterRoute("ForgotPasswordPage", typeof(Views.ForgotPasswordPage));
            Routing.RegisterRoute("MapPage", typeof(Views.MapPage));
        }

        public void ConfigureTabs(string role)
        {
            // Default to Student view
            bool isDriver = role == "driver";

            // Safety check for nulls
            if (DriverTab == null) return;

            // Driver Tab: Only for drivers
            DriverTab.IsVisible = isDriver;

            // Student specific tabs: Hidden for Driver
            if (HomeTab != null) HomeTab.IsVisible = !isDriver;
            if (ScheduleTab != null) ScheduleTab.IsVisible = !isDriver;

            // Common tabs: Visible for everyone
            if (NewsTab != null) NewsTab.IsVisible = true;
            if (ProfileTab != null) ProfileTab.IsVisible = true;
        }
    }
}
