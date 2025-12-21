
import { useEffect, useState } from 'react'
import { MapContainer, TileLayer, Marker, Popup, Polyline, useMap } from 'react-leaflet'
import { supabase } from '../supabaseClient'
import 'leaflet/dist/leaflet.css'
import L from 'leaflet'

// Fix for default marker icon in React Leaflet
import icon from 'leaflet/dist/images/marker-icon.png';
import iconShadow from 'leaflet/dist/images/marker-shadow.png';

let DefaultIcon = L.icon({
    iconUrl: icon,
    shadowUrl: iconShadow,
    iconSize: [25, 41],
    iconAnchor: [12, 41]
});

L.Marker.prototype.options.icon = DefaultIcon;

// CORRECT coordinates from RouteService.cs
const UTS_HOSTEL = [2.3420, 111.8318]
const UTS_CAMPUS = [2.3417, 111.8442]

// Route A: Hostel -> Campus (from RouteService.cs)
const ROUTE_A = [
    [2.3420, 111.8318], // Hostel Start
    [2.3431, 111.8317], // Exit Hostel North
    [2.3435, 111.8340], // Jln Wawasan West
    [2.3433, 111.8365], // Jln Wawasan Mid
    [2.3426, 111.8386], // Curve
    [2.3418, 111.8405], // Pre-Roundabout
    [2.3415, 111.8417], // Roundabout
    [2.3415, 111.8424], // Enter Campus
    [2.3413, 111.8435], // Campus Road
    [2.3417, 111.8442]  // Campus Main
]

// Route B: Campus -> Hostel (reverse)
const ROUTE_B = [...ROUTE_A].reverse()

// Custom icons
const campusIcon = L.divIcon({
    className: 'campus-marker',
    html: '<div style="background:#1a237e;width:44px;height:44px;border-radius:50%;display:flex;align-items:center;justify-content:center;font-size:22px;border:3px solid white;box-shadow:0 2px 10px rgba(0,0,0,0.3);">🏫</div>',
    iconSize: [44, 44],
    iconAnchor: [22, 22]
})

const hostelIcon = L.divIcon({
    className: 'hostel-marker',
    html: '<div style="background:#4CAF50;width:44px;height:44px;border-radius:50%;display:flex;align-items:center;justify-content:center;font-size:22px;border:3px solid white;box-shadow:0 2px 10px rgba(0,0,0,0.3);">🏠</div>',
    iconSize: [44, 44],
    iconAnchor: [22, 22]
})

const busIcon = L.divIcon({
    className: 'bus-marker',
    html: '<div style="background:#ff9800;width:40px;height:40px;border-radius:50%;display:flex;align-items:center;justify-content:center;font-size:20px;border:3px solid white;box-shadow:0 2px 10px rgba(0,0,0,0.3);animation:pulse 2s infinite;">🚌</div>',
    iconSize: [40, 40],
    iconAnchor: [20, 20]
})

// Component to fit bounds on mount
function FitBounds() {
    const map = useMap()
    useEffect(() => {
        const bounds = L.latLngBounds(ROUTE_A)
        map.fitBounds(bounds.pad(0.15)) // Add 15% padding
    }, [map])
    return null
}

export default function MapPage() {
    const [buses, setBuses] = useState([])

    useEffect(() => {
        fetchBuses()

        const channel = supabase
            .channel('live_map')
            .on('postgres_changes', { event: '*', schema: 'public', table: 'bus_locations' }, () => {
                fetchBuses()
            })
            .subscribe()

        const interval = setInterval(fetchBuses, 10000)

        return () => {
            supabase.removeChannel(channel)
            clearInterval(interval)
        }
    }, [])

    async function fetchBuses() {
        const { data } = await supabase
            .from('bus_locations')
            .select('*, buses(*), profiles(*)')
            .eq('status', 'Driving')
        if (data) setBuses(data)
    }

    return (
        <div style={{ height: '100%' }}>
            <h1>Live Map</h1>
            <div style={{ display: 'flex', gap: '1rem', marginBottom: '1rem', flexWrap: 'wrap' }}>
                <span style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                    <span style={{ fontSize: '1.2rem' }}>🏫</span> UTS Campus
                </span>
                <span style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                    <span style={{ fontSize: '1.2rem' }}>🏠</span> Hostel
                </span>
                <span style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                    <span style={{ width: '16px', height: '4px', background: '#2196F3', borderRadius: '2px' }}></span> Route A (Hostel → Campus)
                </span>
                <span style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                    <span style={{ width: '16px', height: '4px', background: '#f44336', borderRadius: '2px' }}></span> Route B (Campus → Hostel)
                </span>
                <span style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                    <span style={{ fontSize: '1.2rem' }}>🚌</span> Active Bus ({buses.length})
                </span>
            </div>
            <div className="card" style={{ height: 'calc(100vh - 220px)', padding: 0, overflow: 'hidden', borderRadius: '16px' }}>
                <MapContainer
                    center={[2.3418, 111.8380]}
                    zoom={16}
                    style={{ height: '100%', width: '100%' }}
                >
                    <FitBounds />
                    <TileLayer
                        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                        attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                    />

                    {/* Route A: Hostel -> Campus (Blue) */}
                    <Polyline
                        positions={ROUTE_A}
                        color="#2196F3"
                        weight={5}
                        opacity={0.8}
                    />

                    {/* Route B: Campus -> Hostel (Red, slightly offset) */}
                    <Polyline
                        positions={ROUTE_B.map(([lat, lng]) => [lat + 0.0001, lng + 0.0001])}
                        color="#f44336"
                        weight={5}
                        opacity={0.8}
                        dashArray="10, 10"
                    />

                    {/* Hostel marker */}
                    <Marker position={UTS_HOSTEL} icon={hostelIcon}>
                        <Popup>
                            <strong>🏠 UTS Hostel</strong><br />
                            Student Accommodation<br />
                            <small>Route A starts here</small>
                        </Popup>
                    </Marker>

                    {/* Campus marker */}
                    <Marker position={UTS_CAMPUS} icon={campusIcon}>
                        <Popup>
                            <strong>🏫 UTS Campus</strong><br />
                            University of Technology Sarawak<br />
                            <small>Route B starts here</small>
                        </Popup>
                    </Marker>

                    {/* Bus markers */}
                    {buses.map(bus => (
                        bus.latitude && bus.longitude && (
                            <Marker key={bus.id} position={[bus.latitude, bus.longitude]} icon={busIcon}>
                                <Popup>
                                    <strong>🚌 {bus.buses?.plate_number || 'Bus'}</strong><br />
                                    Driver: {bus.profiles?.full_name || 'Unknown'}<br />
                                    Route: {bus.route_name || 'Not assigned'}<br />
                                    Status: <span style={{ color: '#4CAF50', fontWeight: 'bold' }}>Driving</span>
                                </Popup>
                            </Marker>
                        )
                    ))}
                </MapContainer>
            </div>

            <style>{`
                @keyframes pulse {
                    0% { transform: scale(1); }
                    50% { transform: scale(1.1); }
                    100% { transform: scale(1); }
                }
            `}</style>
        </div>
    )
}
