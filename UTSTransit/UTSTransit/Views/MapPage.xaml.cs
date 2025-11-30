using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using UTSTransit.Services;
using UTSTransit.ViewModels;

namespace UTSTransit.Views;

public partial class MapPage : ContentPage
{
    private readonly MapViewModel _viewModel;

    public MapPage(TransitService service)
    {
        InitializeComponent();
        _viewModel = new MapViewModel(service);
        BindingContext = _viewModel;

        // 手动同步 ViewModel 的 Pins 到 Map 控件
        _viewModel.BusPins.CollectionChanged += BusPins_CollectionChanged;
    }

    private void BusPins_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
        {
            BusMap.Pins.Clear();
        }

        if (e.OldItems != null)
        {
            foreach (Pin pin in e.OldItems)
            {
                BusMap.Pins.Remove(pin);
            }
        }

        if (e.NewItems != null)
        {
            foreach (Pin pin in e.NewItems)
            {
                if (!BusMap.Pins.Contains(pin))
                    BusMap.Pins.Add(pin);
            }
        }
    }
}