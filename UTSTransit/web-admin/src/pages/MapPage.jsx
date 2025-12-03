
import { useEffect, useState } from 'react'
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet'
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

export default function MapPage() {
    const [buses, setBuses] = useState([])

    useEffect(() => {
        fetchBuses()

        const channel = supabase
            .channel('live_map')
            .on('postgres_changes', { event: '*', schema: 'public', table: 'active_trips' }, payload => {
                console.log('Change received!', payload)
                fetchBuses()
            })
            .subscribe()

        return () => {
            supabase.removeChannel(channel)
        }
    }, [])

    async function fetchBuses() {
        const { data } = await supabase.from('active_trips').select('*')
        if (data) setBuses(data)
    }

    return (
        <div style={{ height: '100%' }}>
            <h1>Live Map</h1>
            <div className="card" style={{ height: 'calc(100vh - 200px)', padding: 0, overflow: 'hidden' }}>
                <MapContainer center={[2.3134, 111.8283]} zoom={15} style={{ height: '100%', width: '100%' }}>
                    <TileLayer
                        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                        attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                    />
                    {buses.map(bus => (
                        <Marker key={bus.id} position={[bus.latitude, bus.longitude]}>
                            <Popup>
                                <strong>{bus.route_name}</strong><br />
                                Status: {bus.status}<br />
                                Last Updated: {new Date(bus.last_updated).toLocaleTimeString()}
                            </Popup>
                        </Marker>
                    ))}
                </MapContainer>
            </div>
        </div>
    )
}
