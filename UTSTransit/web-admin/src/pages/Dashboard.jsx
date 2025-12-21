
import { useEffect, useState } from 'react'
import { supabase } from '../supabaseClient'

export default function Dashboard() {
    const [stats, setStats] = useState({
        students: 0,
        drivers: 0,
        activeBuses: 0,
        todayBookings: 0,
        tomorrowBookings: 0
    })
    const [bookingDetails, setBookingDetails] = useState([])
    const [tomorrowBookingDetails, setTomorrowBookingDetails] = useState([])

    useEffect(() => {
        fetchStats()
        fetchBookingDetails()
        fetchTomorrowBookingDetails()

        // Subscribe to bookings changes
        const channel = supabase
            .channel('dashboard_bookings')
            .on('postgres_changes', { event: '*', schema: 'public', table: 'bookings' }, () => {
                fetchStats()
                fetchBookingDetails()
                fetchTomorrowBookingDetails()
            })
            .subscribe()

        return () => {
            supabase.removeChannel(channel)
        }
    }, [])

    function getMalaysiaDate(offsetDays = 0) {
        const today = new Date()
        const malaysiaOffset = 8 * 60
        const utcOffset = today.getTimezoneOffset()
        const malaysiaTime = new Date(today.getTime() + (malaysiaOffset + utcOffset) * 60000)
        malaysiaTime.setDate(malaysiaTime.getDate() + offsetDays)
        return malaysiaTime.toISOString().split('T')[0]
    }

    async function fetchStats() {
        const { count: studentCount } = await supabase
            .from('profiles')
            .select('*', { count: 'exact', head: true })
            .eq('role', 'student')

        const { count: driverCount } = await supabase
            .from('profiles')
            .select('*', { count: 'exact', head: true })
            .eq('role', 'driver')

        const { count: busCount } = await supabase
            .from('bus_locations')
            .select('*', { count: 'exact', head: true })

        const todayStr = getMalaysiaDate(0)
        const { count: todayCount } = await supabase
            .from('bookings')
            .select('*', { count: 'exact', head: true })
            .eq('booking_date', todayStr)

        const tomorrowStr = getMalaysiaDate(1)
        const { count: tomorrowCount } = await supabase
            .from('bookings')
            .select('*', { count: 'exact', head: true })
            .eq('booking_date', tomorrowStr)

        setStats({
            students: studentCount || 0,
            drivers: driverCount || 0,
            activeBuses: busCount || 0,
            todayBookings: todayCount || 0,
            tomorrowBookings: tomorrowCount || 0
        })
    }

    async function fetchBookingDetails() {
        const dateStr = getMalaysiaDate(0)
        const { data: bookings } = await supabase
            .from('bookings')
            .select('schedule_id')
            .eq('booking_date', dateStr)

        if (!bookings || bookings.length === 0) {
            setBookingDetails([])
            return
        }

        const countMap = {}
        bookings.forEach(b => {
            countMap[b.schedule_id] = (countMap[b.schedule_id] || 0) + 1
        })

        const scheduleIds = Object.keys(countMap)
        const { data: schedules } = await supabase
            .from('schedules')
            .select('id, route_name, departure_time')
            .in('id', scheduleIds)

        const details = schedules?.map(s => ({
            id: s.id,
            name: s.route_name,
            time: s.departure_time?.slice(0, 5),
            count: countMap[s.id] || 0
        })) || []

        setBookingDetails(details.sort((a, b) => a.time?.localeCompare(b.time)))
    }

    async function fetchTomorrowBookingDetails() {
        const dateStr = getMalaysiaDate(1)
        const { data: bookings } = await supabase
            .from('bookings')
            .select('schedule_id')
            .eq('booking_date', dateStr)

        if (!bookings || bookings.length === 0) {
            setTomorrowBookingDetails([])
            return
        }

        const countMap = {}
        bookings.forEach(b => {
            countMap[b.schedule_id] = (countMap[b.schedule_id] || 0) + 1
        })

        const scheduleIds = Object.keys(countMap)
        const { data: schedules } = await supabase
            .from('schedules')
            .select('id, route_name, departure_time')
            .in('id', scheduleIds)

        const details = schedules?.map(s => ({
            id: s.id,
            name: s.route_name,
            time: s.departure_time?.slice(0, 5),
            count: countMap[s.id] || 0
        })) || []

        setTomorrowBookingDetails(details.sort((a, b) => a.time?.localeCompare(b.time)))
    }

    return (
        <div>
            <h1>Dashboard</h1>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: '1rem', marginBottom: '2rem' }}>
                <div className="card">
                    <div className="stat-value">{stats.students}</div>
                    <div className="stat-label">Registered Students</div>
                </div>
                <div className="card">
                    <div className="stat-value">{stats.drivers}</div>
                    <div className="stat-label">Registered Drivers</div>
                </div>
                <div className="card">
                    <div className="stat-value">{stats.activeBuses}</div>
                    <div className="stat-label">Active Buses Now</div>
                </div>
                <div className="card" style={{ background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' }}>
                    <div className="stat-value" style={{ color: '#ffffff' }}>{stats.todayBookings}</div>
                    <div className="stat-label" style={{ color: 'rgba(255,255,255,0.9)' }}>Today's Bookings</div>
                </div>
                <div className="card" style={{ background: 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)' }}>
                    <div className="stat-value" style={{ color: '#ffffff' }}>{stats.tomorrowBookings}</div>
                    <div className="stat-label" style={{ color: 'rgba(255,255,255,0.9)' }}>Tomorrow's Bookings</div>
                </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '2rem' }}>
                <div>
                    <h2>Today's Booking Details</h2>
                    <div className="card">
                        {bookingDetails.length === 0 ? (
                            <p style={{ color: '#888', textAlign: 'center', padding: '2rem' }}>No bookings today</p>
                        ) : (
                            <table>
                                <thead>
                                    <tr>
                                        <th>Time</th>
                                        <th>Route</th>
                                        <th>Passengers</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {bookingDetails.map(item => (
                                        <tr key={item.id}>
                                            <td style={{ fontWeight: 'bold' }}>{item.time}</td>
                                            <td>{item.name}</td>
                                            <td>
                                                <span style={{
                                                    background: '#4CAF50',
                                                    color: 'white',
                                                    padding: '4px 12px',
                                                    borderRadius: '12px',
                                                    fontWeight: 'bold'
                                                }}>
                                                    {item.count}
                                                </span>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        )}
                    </div>
                </div>

                <div>
                    <h2>Tomorrow's Booking Details</h2>
                    <div className="card">
                        {tomorrowBookingDetails.length === 0 ? (
                            <p style={{ color: '#888', textAlign: 'center', padding: '2rem' }}>No bookings for tomorrow</p>
                        ) : (
                            <table>
                                <thead>
                                    <tr>
                                        <th>Time</th>
                                        <th>Route</th>
                                        <th>Passengers</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {tomorrowBookingDetails.map(item => (
                                        <tr key={item.id}>
                                            <td style={{ fontWeight: 'bold' }}>{item.time}</td>
                                            <td>{item.name}</td>
                                            <td>
                                                <span style={{
                                                    background: '#11998e',
                                                    color: 'white',
                                                    padding: '4px 12px',
                                                    borderRadius: '12px',
                                                    fontWeight: 'bold'
                                                }}>
                                                    {item.count}
                                                </span>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        )}
                    </div>
                </div>
            </div>
        </div>
    )
}
