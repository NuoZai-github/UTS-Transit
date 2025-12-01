using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using UTSTransit.Models;
using UTSTransit.Services;

namespace UTSTransit.ViewModels
{
    public partial class TimetableViewModel : ObservableObject
    {
        private readonly RouteService _routeService;

        public ObservableCollection<TimetableItem> Schedule { get; } = new();

        public TimetableViewModel(RouteService routeService)
        {
            _routeService = routeService;
            LoadSchedule();
        }

        private void LoadSchedule()
        {
            var items = _routeService.GetTimetable();
            Schedule.Clear();
            foreach (var item in items)
            {
                Schedule.Add(item);
            }
        }
    }
}
