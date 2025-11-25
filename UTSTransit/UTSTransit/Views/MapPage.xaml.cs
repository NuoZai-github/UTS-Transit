using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Drawing;
using UTSTransit.Models;
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

        _viewModel.BusPins.CollectionChanged += BusPins_CollectionChanged;
    }

    private void BusPins_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
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