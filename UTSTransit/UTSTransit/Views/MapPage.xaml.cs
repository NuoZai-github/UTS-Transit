using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using UTSTransit.Services;
using UTSTransit.ViewModels;

namespace UTSTransit.Views;

public partial class MapPage : ContentPage
{
    private readonly MapViewModel _viewModel;

    public MapPage(MapViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // 手动同步 ViewModel 的 Pins 到 Map 控件
        _viewModel.BusPins.CollectionChanged += BusPins_CollectionChanged;

        // 绘制路线
        DrawRoutes();
    }

    private void DrawRoutes()
    {
        foreach (var route in _viewModel.Routes)
        {
            var polyline = new Polyline
            {
                StrokeColor = route.RouteColor,
                StrokeWidth = 8
            };

            foreach (var coord in route.Coordinates)
            {
                polyline.Geopath.Add(coord);
            }

            BusMap.MapElements.Add(polyline);
        }

        // 移动地图视角到第一条路线的起点
        if (_viewModel.Routes.Count > 0 && _viewModel.Routes[0].Coordinates.Count > 0)
        {
            var firstPoint = _viewModel.Routes[0].Coordinates[0];
            BusMap.MoveToRegion(MapSpan.FromCenterAndRadius(firstPoint, Distance.FromKilometers(1)));
        }
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