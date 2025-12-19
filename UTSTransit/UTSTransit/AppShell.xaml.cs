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

        public void SetDriverTabVisible(bool isVisible)
        {
            if (DriverTab != null)
            {
                DriverTab.IsVisible = isVisible;
            }
        }
        
        public void SetStudentTabsVisible(bool isVisible)
        {
            if (ScheduleTab != null) ScheduleTab.IsVisible = isVisible;
            // Driver needs LiveMap but maybe not Announcements/Profile? 
            // Requests: "Driver don't need to see this page" (Referring to image of Schedule page likely)
            // Let's hide Schedule tab for Drivers.
        }
    }
}
