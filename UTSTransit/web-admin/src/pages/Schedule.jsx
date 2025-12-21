
import { useEffect, useState } from 'react'
import { supabase } from '../supabaseClient'

export default function Schedule() {
    const [items, setItems] = useState([])
    const [bookingCounts, setBookingCounts] = useState({})
    const [showModal, setShowModal] = useState(false)
    const [selectedDate, setSelectedDate] = useState(getTodayString())
    const [formData, setFormData] = useState({
        route_name: '',
        departure_time: '',
        slot_type: 'Daily',
        special_reason: ''
    })

    function getTodayString() {
        const today = new Date()
        const malaysiaOffset = 8 * 60
        const utcOffset = today.getTimezoneOffset()
        const malaysiaTime = new Date(today.getTime() + (malaysiaOffset + utcOffset) * 60000)
        return malaysiaTime.toISOString().split('T')[0]
    }

    function isWeekend(dateStr) {
        const date = new Date(dateStr)
        const day = date.getDay()
        return day === 0 || day === 6 // Sunday = 0, Saturday = 6
    }

    function getDayName(dateStr) {
        const date = new Date(dateStr)
        return date.toLocaleDateString('en-US', { weekday: 'long' })
    }

    useEffect(() => {
        fetchItems()
        fetchBookingCounts()

        const interval = setInterval(() => {
            fetchItems()
            fetchBookingCounts()
        }, 30000)

        return () => clearInterval(interval)
    }, [selectedDate])

    async function fetchItems() {
        const { data, error } = await supabase.rpc('get_schedules_with_status')

        if (error) {
            console.log('RPC not available, using direct query:', error.message)
            const { data: directData } = await supabase.from('schedules').select('*').order('departure_time', { ascending: true })
            if (directData) {
                // Get Malaysia current date and time
                const malaysiaOffset = 8 * 60
                const now = new Date()
                const utcOffset = now.getTimezoneOffset()
                const malaysiaTime = new Date(now.getTime() + (malaysiaOffset + utcOffset) * 60000)
                const todayStr = malaysiaTime.toISOString().split('T')[0]
                const currentTime = malaysiaTime.toTimeString().slice(0, 5)

                const itemsWithStatus = directData.map(item => {
                    // Compare selectedDate with today
                    if (selectedDate > todayStr) {
                        // Future date - always open
                        return { ...item, is_closed: false }
                    } else if (selectedDate < todayStr) {
                        // Past date - always closed
                        return { ...item, is_closed: true }
                    } else {
                        // Today - calculate based on time
                        const deptTime = item.departure_time?.slice(0, 5)
                        const [deptH, deptM] = deptTime.split(':').map(Number)
                        const cutoffMinutes = deptH * 60 + deptM - 10
                        const [currH, currM] = currentTime.split(':').map(Number)
                        const currentMinutes = currH * 60 + currM
                        return {
                            ...item,
                            is_closed: currentMinutes > cutoffMinutes
                        }
                    }
                })
                setItems(itemsWithStatus)
            }
        } else {
            // If RPC succeeded, still need to adjust based on selectedDate
            const malaysiaOffset = 8 * 60
            const now = new Date()
            const utcOffset = now.getTimezoneOffset()
            const malaysiaTime = new Date(now.getTime() + (malaysiaOffset + utcOffset) * 60000)
            const todayStr = malaysiaTime.toISOString().split('T')[0]
            const currentTime = malaysiaTime.toTimeString().slice(0, 5)

            const itemsWithStatus = (data || []).map(item => {
                if (selectedDate > todayStr) {
                    return { ...item, is_closed: false }
                } else if (selectedDate < todayStr) {
                    return { ...item, is_closed: true }
                } else {
                    const deptTime = item.departure_time?.slice(0, 5)
                    const [deptH, deptM] = deptTime.split(':').map(Number)
                    const cutoffMinutes = deptH * 60 + deptM - 10
                    const [currH, currM] = currentTime.split(':').map(Number)
                    const currentMinutes = currH * 60 + currM
                    return {
                        ...item,
                        is_closed: currentMinutes > cutoffMinutes
                    }
                }
            })
            setItems(itemsWithStatus)
        }
    }

    async function fetchBookingCounts() {
        // Use selectedDate instead of today
        const { data } = await supabase
            .from('bookings')
            .select('schedule_id')
            .eq('booking_date', selectedDate)

        if (data) {
            const counts = {}
            data.forEach(b => {
                counts[b.schedule_id] = (counts[b.schedule_id] || 0) + 1
            })
            setBookingCounts(counts)
        } else {
            setBookingCounts({})
        }
    }

    async function handleSubmit(e) {
        e.preventDefault()

        const isSpecial = formData.slot_type === 'Special'
        const dayType = isSpecial ? 'Special' : 'Weekday'

        const insertData = {
            route_name: formData.route_name,
            departure_time: formData.departure_time,
            day_type: dayType,
            status: isSpecial ? `Special: ${formData.special_reason}` : 'Scheduled',
            // For Special slots, save the selected date so it only shows on that specific day
            special_date: isSpecial ? selectedDate : null
        }

        const { error } = await supabase.from('schedules').insert([insertData])

        if (!error) {
            setShowModal(false)
            setFormData({ route_name: '', departure_time: '', slot_type: 'Daily', special_reason: '' })
            fetchItems()

            if (!isSpecial) {
                alert('✅ Daily slot added! This will appear for all weekdays (Mon-Fri).')
            } else {
                alert(`✅ Special slot added for ${selectedDate}!`)
            }
        } else {
            alert(error.message)
        }
    }

    async function deleteItem(id, dayType) {
        const isSpecial = dayType === 'Special'

        if (isSpecial) {
            if (!confirm('Delete this special slot?')) return
        } else {
            if (!confirm('⚠️ This is a DAILY slot!\n\nDeleting will remove this schedule for ALL weekdays.\n\nAre you sure?')) return
        }

        await supabase.from('schedules').delete().eq('id', id)
        fetchItems()
    }

    function formatTime(timeStr) {
        if (!timeStr) return ''
        const [h, m] = timeStr.slice(0, 5).split(':').map(Number)
        const ampm = h >= 12 ? 'PM' : 'AM'
        const hour12 = h % 12 || 12
        return `${hour12}:${m.toString().padStart(2, '0')} ${ampm}`
    }

    function openAddModal() {
        const isWeekendDay = isWeekend(selectedDate)
        setFormData({
            route_name: '',
            departure_time: '',
            slot_type: isWeekendDay ? 'Special' : 'Daily',
            special_reason: ''
        })
        setShowModal(true)
    }

    const isWeekendSelected = isWeekend(selectedDate)

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
                <h1>Bus Schedule</h1>
                <button className="btn btn-primary" onClick={openAddModal}>+ Add Trip</button>
            </div>

            {/* Date Selector */}
            <div style={{ display: 'flex', gap: '1rem', alignItems: 'center', marginBottom: '1rem' }}>
                <label style={{ fontWeight: 'bold' }}>Select Date:</label>
                <input
                    type="date"
                    value={selectedDate}
                    onChange={e => setSelectedDate(e.target.value)}
                    style={{ padding: '8px 12px', borderRadius: '8px', border: '1px solid #ddd' }}
                />
                <span style={{
                    padding: '4px 12px',
                    borderRadius: '12px',
                    background: isWeekendSelected ? '#ff9800' : '#4CAF50',
                    color: 'white',
                    fontWeight: 'bold'
                }}>
                    {getDayName(selectedDate)} {isWeekendSelected ? '(Weekend)' : '(Weekday)'}
                </span>
            </div>

            <p style={{ color: '#888', marginBottom: '1rem', fontSize: '0.9rem' }}>
                ⏰ Schedules auto-close <strong>10 minutes before departure</strong> (Malaysia Time UTC+8)
            </p>

            {/* Legend */}
            <div style={{ display: 'flex', gap: '1rem', marginBottom: '1rem' }}>
                <span style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                    <span style={{ width: '12px', height: '12px', background: '#4CAF50', borderRadius: '50%' }}></span>
                    Daily (Mon-Fri)
                </span>
                <span style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                    <span style={{ width: '12px', height: '12px', background: '#ff9800', borderRadius: '50%' }}></span>
                    Special Event
                </span>
            </div>

            <div className="card">
                <table>
                    <thead>
                        <tr>
                            <th>Time</th>
                            <th>Route</th>
                            <th>Type</th>
                            <th>Status/Reason</th>
                            <th>Bookings</th>
                            <th>Open/Closed</th>
                            <th>Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        {items
                            .filter(item => {
                                // 1. Special slots: Only show if they match the SELECTED DATE
                                if (item.day_type === 'Special') {
                                    return item.special_date === selectedDate
                                }

                                // 2. Daily slots: Only show on WEEKDAYS (Mon-Fri)
                                // They should NOT appear on weekends
                                return !isWeekendSelected
                            })
                            .map(item => {
                                const isSpecial = item.day_type === 'Special'
                                return (
                                    <tr key={item.id} style={{ opacity: item.is_closed ? 0.5 : 1 }}>
                                        <td style={{ fontWeight: 'bold' }}>{formatTime(item.departure_time)}</td>
                                        <td>{item.route_name}</td>
                                        <td>
                                            <span style={{
                                                padding: '2px 8px',
                                                borderRadius: '8px',
                                                background: isSpecial ? '#ff9800' : '#4CAF50',
                                                color: 'white',
                                                fontSize: '0.8rem'
                                            }}>
                                                {isSpecial ? 'Special' : 'Daily'}
                                            </span>
                                        </td>
                                        <td>{item.status}</td>
                                        <td>
                                            <span style={{
                                                background: bookingCounts[item.id] > 0 ? '#4CAF50' : '#ddd',
                                                color: bookingCounts[item.id] > 0 ? 'white' : '#666',
                                                padding: '2px 8px',
                                                borderRadius: '10px',
                                                fontSize: '0.85rem'
                                            }}>
                                                {bookingCounts[item.id] || 0}
                                            </span>
                                        </td>
                                        <td>
                                            {item.is_closed ? (
                                                <span style={{ color: '#dc3545', fontWeight: 'bold' }}>🔒 Closed</span>
                                            ) : (
                                                <span style={{ color: '#28a745', fontWeight: 'bold' }}>✅ Open</span>
                                            )}
                                        </td>
                                        <td>
                                            <button
                                                className="btn btn-danger"
                                                style={{ padding: '4px 8px', fontSize: '0.8rem' }}
                                                onClick={() => deleteItem(item.id, item.day_type)}
                                            >
                                                Remove
                                            </button>
                                        </td>
                                    </tr>
                                )
                            })}
                        {items.filter(item => isWeekendSelected ? item.day_type === 'Special' : true).length === 0 && (
                            <tr>
                                <td colSpan="7" style={{ textAlign: 'center', padding: '2rem', color: '#888' }}>
                                    {isWeekendSelected
                                        ? '📅 No scheduled trips for this weekend. Add a Special slot if needed.'
                                        : '📅 No scheduled trips. Click "+ Add Trip" to create one.'}
                                </td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </div>

            {showModal && (
                <div className="modal-overlay" onClick={() => setShowModal(false)}>
                    <div className="modal" onClick={e => e.stopPropagation()}>
                        <h2>Add Schedule for {getDayName(selectedDate)}</h2>

                        <div style={{
                            padding: '12px',
                            background: isWeekendSelected ? '#fff3e0' : '#e8f5e9',
                            borderRadius: '8px',
                            marginBottom: '1rem'
                        }}>
                            {isWeekendSelected ? (
                                <p style={{ margin: 0, color: '#e65100' }}>
                                    ⚠️ <strong>{getDayName(selectedDate)}</strong> is a weekend. Only Special slots can be added.
                                </p>
                            ) : (
                                <p style={{ margin: 0, color: '#2e7d32' }}>
                                    ✅ <strong>{getDayName(selectedDate)}</strong> is a weekday. You can add Daily or Special slots.
                                </p>
                            )}
                        </div>

                        <form onSubmit={handleSubmit}>
                            <select
                                value={formData.route_name}
                                onChange={e => setFormData({ ...formData, route_name: e.target.value })}
                                required
                            >
                                <option value="">Select Route</option>
                                <option value="Route A (Hostel -> Campus)">Route A (Hostel → Campus)</option>
                                <option value="Route B (Campus -> Hostel)">Route B (Campus → Hostel)</option>
                            </select>

                            <input
                                type="time"
                                value={formData.departure_time}
                                onChange={e => setFormData({ ...formData, departure_time: e.target.value })}
                                required
                            />

                            {!isWeekendSelected && (
                                <select
                                    value={formData.slot_type}
                                    onChange={e => setFormData({ ...formData, slot_type: e.target.value })}
                                >
                                    <option value="Daily">Daily Slot (Mon-Fri)</option>
                                    <option value="Special">Special Slot (One-time)</option>
                                </select>
                            )}

                            {(formData.slot_type === 'Special' || isWeekendSelected) && (
                                <input
                                    type="text"
                                    placeholder="Reason for special slot (e.g., Exam Day, Event)"
                                    value={formData.special_reason}
                                    onChange={e => setFormData({ ...formData, special_reason: e.target.value })}
                                    required
                                />
                            )}

                            {formData.slot_type === 'Daily' && !isWeekendSelected && (
                                <p style={{ fontSize: '0.85rem', color: '#666', margin: '0.5rem 0' }}>
                                    ℹ️ Daily slots will appear for ALL weekdays (Monday to Friday)
                                </p>
                            )}

                            <div style={{ display: 'flex', gap: '1rem', justifyContent: 'flex-end' }}>
                                <button type="button" className="btn" onClick={() => setShowModal(false)}>Cancel</button>
                                <button type="submit" className="btn btn-primary">Add</button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    )
}
