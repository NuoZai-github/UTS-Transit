using Mapsui.UI.Maui;
using Mapsui.Projections;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Nts; // For GeometryFeature
using NetTopologySuite.Geometries; // For Point, LineString
using UTSTransit.Services;
using UTSTransit.ViewModels;
using Microsoft.Maui.Devices.Sensors; // For Location struct
using Color = Microsoft.Maui.Graphics.Color;

namespace UTSTransit.Views;

public partial class MapPage : ContentPage
{
    private readonly MapViewModel _viewModel;
    private MemoryLayer _routesLayer;

    public MapPage(MapViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Initialize Map
        InitializeMap();

        // Subscribe to Pin updates
        _viewModel.BusPins.CollectionChanged += BusPins_CollectionChanged;
    }

    private void InitializeMap()
    {
        BusMap.Map ??= new Mapsui.Map();
        
        // Add OSM Layer
        BusMap.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

        // Create Layer for Routes
        _routesLayer = new MemoryLayer
        {
            Name = "Routes",
            Style = null // Style is on features
        };
        BusMap.Map.Layers.Add(_routesLayer);

        // Draw Routes
        DrawRoutes();

        // Center Map on UTS Sibu
        // Mapsui v5 SphericalMercator.FromLonLat returns (double x, double y) tuple
        var (sibuX, sibuY) = SphericalMercator.FromLonLat(111.8283, 2.3134);
        var sibuCenter = new MPoint(sibuX, sibuY);
        
        BusMap.Map.Navigator.CenterOn(sibuCenter);
        BusMap.Map.Navigator.ZoomTo(2000); // Resolution, lower is zoomed in
    }

    private void DrawRoutes()
    {
        var features = new List<IFeature>();

        foreach (var route in _viewModel.Routes)
        {
            var points = new List<Coordinate>();
            foreach (var coord in route.Coordinates)
            {
                // Convert Lat/Lon to Spherical Mercator
                var (x, y) = SphericalMercator.FromLonLat(coord.Longitude, coord.Latitude);
                points.Add(new Coordinate(x, y));
            }

            var lineString = new LineString(points.ToArray());
            var feature = new GeometryFeature
            {
                Geometry = lineString
            };

            // Style
            var color = ToMapsuiColor(route.RouteColor);
            feature.Styles.Add(new VectorStyle
            {
                Line = new Pen { Color = color, Width = 5 }
            });

            features.Add(feature);
        }

        _routesLayer.Features = features;
    }

    private Mapsui.Styles.Color ToMapsuiColor(Color mauiColor)
    {
        return new Mapsui.Styles.Color(
            (int)(mauiColor.Red * 255),
            (int)(mauiColor.Green * 255),
            (int)(mauiColor.Blue * 255),
            (int)(mauiColor.Alpha * 255));
    }

    private void BusPins_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        BusMap.Pins.Clear();

        foreach (var busPin in _viewModel.BusPins)
        {
            var pin = new Pin(BusMap)
            {
                Position = new Mapsui.UI.Maui.Position(busPin.Latitude, busPin.Longitude),
                Label = busPin.Label,
                Address = busPin.Address,
                Type = PinType.Pin, // Changed from Icon to Pin
                Scale = 0.7f,
                Color = Microsoft.Maui.Graphics.Colors.Blue // Default color
            };
            BusMap.Pins.Add(pin);
        }
    }
}