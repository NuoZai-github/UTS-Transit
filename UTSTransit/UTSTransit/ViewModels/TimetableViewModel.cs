using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using UTSTransit.Models;
using UTSTransit.Services;

namespace UTSTransit.ViewModels
{
    public partial class TimetableViewModel : ObservableObject
    {
        private readonly TransitService _transitService;

        public ObservableCollection<TimetableItem> Schedule { get; } = new();

        public TimetableViewModel(TransitService transitService)
        {
            _transitService = transitService;
            LoadSchedule();
        }

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private async Task Book(TimetableItem item)
        {
            if (item.IsBooked) return;

            var result = await _transitService.BookSlotAsync(item.ScheduleId);
            if (result.IsSuccess)
            {
                await Application.Current.MainPage.DisplayAlert("Success", "Trip booked successfully!", "OK");
                LoadSchedule(); // Refresh to update status
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", result.Message, "OK");
            }
        }

        private async void LoadSchedule()
        {
            var schedules = await _transitService.GetSchedulesAsync();
            var myBookings = await _transitService.GetStudentBookingsAsync();
            var bookedscheduleIds = myBookings.Select(b => b.ScheduleId).ToHashSet();

            Schedule.Clear();
            foreach (var item in schedules)
            {
                var isBooked = bookedscheduleIds.Contains(item.Id);
                Schedule.Add(new TimetableItem
                {
                    ScheduleId = item.Id,
                    RouteName = item.RouteName,
                    DepartureTime = DateTime.Today.Add(item.DepartureTime).ToString("hh:mm tt"),
                    Status = item.Status,
                    IsBooked = isBooked
                });
            }
        }
    }
}
