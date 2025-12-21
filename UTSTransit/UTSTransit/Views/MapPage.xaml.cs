using Mapsui.UI.Maui;
using Mapsui.Projections;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Nts;
using NetTopologySuite.Geometries;
using UTSTransit.Services;
using UTSTransit.ViewModels;
using Microsoft.Maui.Devices.Sensors;
using Color = Microsoft.Maui.Graphics.Color;

namespace UTSTransit.Views;

public partial class MapPage : ContentPage
{
    private readonly MapViewModel _viewModel;
    private MemoryLayer _routesLayer;

    // CORRECT coordinates from RouteService.cs
    private const double UTS_HOSTEL_LAT = 2.3420;
    private const double UTS_HOSTEL_LON = 111.8318;
    private const double UTS_CAMPUS_LAT = 2.3417;
    private const double UTS_CAMPUS_LON = 111.8442;

    public MapPage(MapViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        InitializeMap();
        _viewModel.BusPins.CollectionChanged += BusPins_CollectionChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CheckAndRequestLocationPermission();
    }

    private async Task CheckAndRequestLocationPermission()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MapPage Permission request failed: {ex.Message}");
        }
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
            Style = null
        };
        BusMap.Map.Layers.Add(_routesLayer);

        // Draw Routes from ViewModel (which uses RouteService)
        DrawRoutes();

        // Add static pins for Hostel and Campus
        AddLocationPins();

        // Zoom to fit the entire route
        ZoomToRoute();
    }

    private void ZoomToRoute()
    {
        // Get all route coordinates to calculate bounds
        double minLat = double.MaxValue, maxLat = double.MinValue;
        double minLon = double.MaxValue, maxLon = double.MinValue;

        foreach (var route in _viewModel.Routes)
        {
            foreach (var coord in route.Coordinates)
            {
                minLat = Math.Min(minLat, coord.Latitude);
                maxLat = Math.Max(maxLat, coord.Latitude);
                minLon = Math.Min(minLon, coord.Longitude);
                maxLon = Math.Max(maxLon, coord.Longitude);
            }
        }

        // Fallback if no routes
        if (minLat == double.MaxValue)
        {
            minLat = Math.Min(UTS_HOSTEL_LAT, UTS_CAMPUS_LAT);
            maxLat = Math.Max(UTS_HOSTEL_LAT, UTS_CAMPUS_LAT);
            minLon = Math.Min(UTS_HOSTEL_LON, UTS_CAMPUS_LON);
            maxLon = Math.Max(UTS_HOSTEL_LON, UTS_CAMPUS_LON);
        }

        // Convert corners to Spherical Mercator
        var (minX, minY) = SphericalMercator.FromLonLat(minLon, minLat);
        var (maxX, maxY) = SphericalMercator.FromLonLat(maxLon, maxLat);

        // Add 15% padding
        var paddingX = (maxX - minX) * 0.15;
        var paddingY = (maxY - minY) * 0.15;

        // Create bounding box
        var boundingBox = new MRect(
            minX - paddingX,
            minY - paddingY,
            maxX + paddingX,
            maxY + paddingY
        );

        // Navigate to bounding box
        BusMap.Map.Navigator.ZoomToBox(boundingBox);
    }

    private void AddLocationPins()
    {
        // Hostel Pin (Green)
        var hostelPin = new Pin(BusMap)
        {
            Position = new Mapsui.UI.Maui.Position(UTS_HOSTEL_LAT, UTS_HOSTEL_LON),
            Label = "UTS Hostel",
            Address = "Student Accommodation",
            Type = PinType.Pin,
            Scale = 0.8f,
            Color = Microsoft.Maui.Graphics.Colors.Green
        };
        BusMap.Pins.Add(hostelPin);

        // Campus Pin (Blue)
        var campusPin = new Pin(BusMap)
        {
            Position = new Mapsui.UI.Maui.Position(UTS_CAMPUS_LAT, UTS_CAMPUS_LON),
            Label = "UTS Campus",
            Address = "University of Technology Sarawak",
            Type = PinType.Pin,
            Scale = 0.8f,
            Color = Microsoft.Maui.Graphics.Colors.DarkBlue
        };
        BusMap.Pins.Add(campusPin);
    }

    private void DrawRoutes()
    {
        var features = new List<IFeature>();

        foreach (var route in _viewModel.Routes)
        {
            var points = new List<Coordinate>();
            foreach (var coord in route.Coordinates)
            {
                var (x, y) = SphericalMercator.FromLonLat(coord.Longitude, coord.Latitude);
                points.Add(new Coordinate(x, y));
            }

            if (points.Count >= 2)
            {
                var lineString = new LineString(points.ToArray());
                var feature = new GeometryFeature { Geometry = lineString };
                var color = ToMapsuiColor(route.RouteColor);
                feature.Styles.Add(new VectorStyle
                {
                    Line = new Pen { Color = color, Width = 5 }
                });
                features.Add(feature);
            }
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
        // Remove only bus pins, keep location pins
        var pinsToRemove = BusMap.Pins.Where(p => p.Label != "UTS Campus" && p.Label != "UTS Hostel").ToList();
        foreach (var pin in pinsToRemove)
        {
            BusMap.Pins.Remove(pin);
        }

        foreach (var busPin in _viewModel.BusPins)
        {
            var pin = new Pin(BusMap)
            {
                Position = new Mapsui.UI.Maui.Position(busPin.Latitude, busPin.Longitude),
                Label = busPin.Label,
                Address = busPin.Address,
                Type = PinType.Pin,
                Scale = 0.7f,
                Color = Microsoft.Maui.Graphics.Colors.Orange
            };
            BusMap.Pins.Add(pin);
        }
    }
}