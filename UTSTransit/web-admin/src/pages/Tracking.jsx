
import { useEffect, useState } from 'react'
import { supabase } from '../supabaseClient'

export default function Tracking() {
    const [busLocations, setBusLocations] = useState([])
    const [selectedBus, setSelectedBus] = useState(null)

    useEffect(() => {
        fetchBusLocations()

        // Real-time subscription to bus locations
        const channel = supabase
            .channel('bus_tracking')
            .on('postgres_changes', {
                event: '*',
                schema: 'public',
                table: 'bus_locations'
            }, () => {
                fetchBusLocations()
            })
            .subscribe()

        // Refresh every 10 seconds
        const interval = setInterval(fetchBusLocations, 10000)

        return () => {
            supabase.removeChannel(channel)
            clearInterval(interval)
        }
    }, [])

    async function fetchBusLocations() {
        const { data } = await supabase
            .from('bus_locations')
            .select('*') // Simplified query
            .order('updated_at', { ascending: false })

        if (data) {
            setBusLocations(data)
        }
    }

    function getTimeSince(timestamp) {
        if (!timestamp) return 'Unknown'
        const now = new Date()
        const updated = new Date(timestamp)
        const diffMs = now - updated
        const diffMins = Math.floor(diffMs / 60000)

        if (diffMins < 1) return 'Just now'
        if (diffMins < 60) return `${diffMins} min ago`
        return `${Math.floor(diffMins / 60)}h ago`
    }

    function getStatusInfo(status) {
        switch (status) {
            case 'Driving':
                return { color: '#4CAF50', icon: '🚌', label: 'Driving' }
            case 'Resting':
                return { color: '#ff9800', icon: '😴', label: 'Resting' }
            case 'Offline':
            default:
                return { color: '#888', icon: '⭕', label: 'Offline' }
        }
    }

    // Filter to only show active drivers (Driving or Resting)
    const activeBuses = busLocations.filter(b => b.status === 'Driving' || b.status === 'Resting')
    const drivingBuses = busLocations.filter(b => b.status === 'Driving')

    return (
        <div>
            <h1>Live Bus Tracking</h1>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 350px', gap: '1.5rem', marginTop: '1rem' }}>
                {/* Map Section */}
                <div className="card" style={{ padding: 0, overflow: 'hidden', minHeight: '500px' }}>
                    <div style={{
                        background: 'linear-gradient(135deg, #e0f2f1 0%, #b2dfdb 100%)',
                        height: '100%',
                        display: 'flex',
                        flexDirection: 'column',
                        alignItems: 'center',
                        justifyContent: 'center',
                        position: 'relative'
                    }}>
                        {/* Simple visual map representation */}
                        <div style={{
                            width: '90%',
                            height: '90%',
                            background: 'white',
                            borderRadius: '16px',
                            padding: '2rem',
                            position: 'relative',
                            boxShadow: '0 4px 20px rgba(0,0,0,0.1)'
                        }}>
                            {/* Route Line */}
                            <svg style={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%', pointerEvents: 'none' }}>
                                <line
                                    x1="20%" y1="30%"
                                    x2="80%" y2="70%"
                                    stroke="#1a237e"
                                    strokeWidth="4"
                                    strokeDasharray="10,5"
                                />
                            </svg>

                            {/* Campus Marker */}
                            <div style={{
                                position: 'absolute',
                                left: '20%',
                                top: '30%',
                                transform: 'translate(-50%, -50%)'
                            }}>
                                <div style={{
                                    width: '60px',
                                    height: '60px',
                                    background: '#1a237e',
                                    borderRadius: '50%',
                                    display: 'flex',
                                    alignItems: 'center',
                                    justifyContent: 'center',
                                    color: 'white',
                                    fontWeight: 'bold',
                                    fontSize: '24px',
                                    boxShadow: '0 4px 15px rgba(26,35,126,0.4)'
                                }}>
                                    🏫
                                </div>
                                <div style={{ textAlign: 'center', marginTop: '8px', fontWeight: 'bold', color: '#1a237e' }}>
                                    Campus
                                </div>
                            </div>

                            {/* Hostel Marker */}
                            <div style={{
                                position: 'absolute',
                                left: '80%',
                                top: '70%',
                                transform: 'translate(-50%, -50%)'
                            }}>
                                <div style={{
                                    width: '60px',
                                    height: '60px',
                                    background: '#4CAF50',
                                    borderRadius: '50%',
                                    display: 'flex',
                                    alignItems: 'center',
                                    justifyContent: 'center',
                                    color: 'white',
                                    fontWeight: 'bold',
                                    fontSize: '24px',
                                    boxShadow: '0 4px 15px rgba(76,175,80,0.4)'
                                }}>
                                    🏠
                                </div>
                                <div style={{ textAlign: 'center', marginTop: '8px', fontWeight: 'bold', color: '#4CAF50' }}>
                                    Hostel
                                </div>
                            </div>

                            {/* Bus Locations - only show driving buses on map */}
                            {drivingBuses.map((bus, index) => {
                                // Simulate position along the route
                                const progress = ((index + 1) / (drivingBuses.length + 1))
                                const left = 20 + (progress * 60)
                                const top = 30 + (progress * 40)
                                const statusInfo = getStatusInfo(bus.status)

                                return (
                                    <div
                                        key={bus.id}
                                        style={{
                                            position: 'absolute',
                                            left: `${left}%`,
                                            top: `${top}%`,
                                            transform: 'translate(-50%, -50%)',
                                            cursor: 'pointer',
                                            transition: 'all 0.3s ease'
                                        }}
                                        onClick={() => setSelectedBus(bus)}
                                    >
                                        <div style={{
                                            width: '50px',
                                            height: '50px',
                                            background: statusInfo.color,
                                            borderRadius: '50%',
                                            display: 'flex',
                                            alignItems: 'center',
                                            justifyContent: 'center',
                                            color: 'white',
                                            fontSize: '24px',
                                            boxShadow: `0 4px 15px ${statusInfo.color}80`,
                                            border: selectedBus?.id === bus.id ? '3px solid #1a237e' : 'none',
                                            animation: 'pulse 2s infinite'
                                        }}>
                                            🚌
                                        </div>
                                        <div style={{
                                            textAlign: 'center',
                                            marginTop: '4px',
                                            fontSize: '12px',
                                            fontWeight: 'bold',
                                            color: '#333'
                                        }}>
                                            {bus.buses?.plate_number || 'Bus'}
                                        </div>
                                        <div style={{
                                            textAlign: 'center',
                                            fontSize: '10px',
                                            color: statusInfo.color,
                                            fontWeight: 'bold'
                                        }}>
                                            {bus.route_name || ''}
                                        </div>
                                    </div>
                                )
                            })}

                            {/* No buses message */}
                            {drivingBuses.length === 0 && (
                                <div style={{
                                    position: 'absolute',
                                    top: '50%',
                                    left: '50%',
                                    transform: 'translate(-50%, -50%)',
                                    textAlign: 'center',
                                    color: '#888'
                                }}>
                                    <div style={{ fontSize: '48px', marginBottom: '1rem' }}>🚌💤</div>
                                    <p>No buses currently driving</p>
                                </div>
                            )}
                        </div>
                    </div>
                </div>

                {/* Bus List Section */}
                <div>
                    <div className="card" style={{ marginBottom: '1rem' }}>
                        <h3 style={{ marginTop: 0 }}>Driver Status</h3>

                        {activeBuses.length === 0 ? (
                            <p style={{ color: '#888', textAlign: 'center' }}>No drivers are currently active</p>
                        ) : (
                            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                                {activeBuses.map(bus => {
                                    const statusInfo = getStatusInfo(bus.status)
                                    return (
                                        <div
                                            key={bus.id}
                                            onClick={() => setSelectedBus(bus)}
                                            style={{
                                                padding: '1rem',
                                                background: selectedBus?.id === bus.id ? '#e3f2fd' : '#f8f9fa',
                                                borderRadius: '12px',
                                                cursor: 'pointer',
                                                border: selectedBus?.id === bus.id ? '2px solid #1a237e' : '2px solid transparent',
                                                transition: 'all 0.2s ease'
                                            }}
                                        >
                                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                                <div>
                                                    <div style={{ fontWeight: 'bold', fontSize: '1.1rem' }}>
                                                        {statusInfo.icon} {bus.buses?.plate_number || 'Unknown Bus'}
                                                    </div>
                                                    <div style={{ fontSize: '0.85rem', color: '#666', marginTop: '4px' }}>
                                                        Driver: {bus.profiles?.full_name || 'Unknown'}
                                                    </div>
                                                    {bus.route_name && (
                                                        <div style={{ fontSize: '0.8rem', color: '#1a237e', marginTop: '2px' }}>
                                                            📍 {bus.route_name}
                                                        </div>
                                                    )}
                                                </div>
                                                <div style={{ textAlign: 'right' }}>
                                                    <div style={{
                                                        padding: '4px 10px',
                                                        borderRadius: '12px',
                                                        background: statusInfo.color,
                                                        color: 'white',
                                                        fontSize: '0.8rem',
                                                        fontWeight: 'bold'
                                                    }}>
                                                        {statusInfo.label}
                                                    </div>
                                                    <div style={{ fontSize: '0.75rem', color: '#888', marginTop: '4px' }}>
                                                        {getTimeSince(bus.updated_at)}
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    )
                                })}
                            </div>
                        )}
                    </div>

                    {/* Selected Bus Details */}
                    {selectedBus && (
                        <div className="card" style={{ background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' }}>
                            <h3 style={{ color: 'white', marginTop: 0 }}>Bus Details</h3>
                            <div style={{ color: 'rgba(255,255,255,0.9)' }}>
                                <p><strong>Plate:</strong> {selectedBus.buses?.plate_number}</p>
                                <p><strong>Driver:</strong> {selectedBus.profiles?.full_name}</p>
                                <p><strong>Status:</strong> {getStatusInfo(selectedBus.status).label}</p>
                                <p><strong>Route:</strong> {selectedBus.route_name || 'Not assigned'}</p>
                                <p><strong>Last Update:</strong> {getTimeSince(selectedBus.updated_at)}</p>
                                {selectedBus.latitude && selectedBus.longitude && (
                                    <>
                                        <p><strong>Lat:</strong> {selectedBus.latitude?.toFixed(6)}</p>
                                        <p><strong>Lng:</strong> {selectedBus.longitude?.toFixed(6)}</p>
                                    </>
                                )}
                            </div>
                        </div>
                    )}

                    {/* Legend */}
                    <div className="card">
                        <h4 style={{ marginTop: 0 }}>Status Legend</h4>
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                                <span style={{ fontSize: '1.2rem' }}>🚌</span>
                                <span style={{ color: '#4CAF50', fontWeight: 'bold' }}>Driving</span>
                                <span style={{ color: '#888', fontSize: '0.9rem' }}>- Bus is on route</span>
                            </div>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                                <span style={{ fontSize: '1.2rem' }}>😴</span>
                                <span style={{ color: '#ff9800', fontWeight: 'bold' }}>Resting</span>
                                <span style={{ color: '#888', fontSize: '0.9rem' }}>- Driver is taking a break</span>
                            </div>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                                <span style={{ fontSize: '1.2rem' }}>⭕</span>
                                <span style={{ color: '#888', fontWeight: 'bold' }}>Offline</span>
                                <span style={{ color: '#888', fontSize: '0.9rem' }}>- Driver is not active</span>
                            </div>
                        </div>
                    </div>
                </div>
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
