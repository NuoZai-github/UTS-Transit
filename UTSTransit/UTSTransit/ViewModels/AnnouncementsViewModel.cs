using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using UTSTransit.Models;
using UTSTransit.Services;

namespace UTSTransit.ViewModels
{
    public partial class AnnouncementsViewModel : ObservableObject
    {
        private readonly TransitService _transitService;

        public ObservableCollection<Announcement> Announcements { get; } = new();

        public AnnouncementsViewModel(TransitService transitService)
        {
            _transitService = transitService;
            LoadAnnouncements();
        }

        public async void LoadAnnouncements()
        {
            var items = await _transitService.GetAnnouncementsAsync();
            Announcements.Clear();
            foreach (var item in items)
            {
                Announcements.Add(item);
            }
        }
    }
}
