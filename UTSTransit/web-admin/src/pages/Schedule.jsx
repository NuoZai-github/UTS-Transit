
import { useEffect, useState } from 'react'
import { supabase } from '../supabaseClient'

export default function Schedule() {
    const [items, setItems] = useState([])
    const [showModal, setShowModal] = useState(false)
    const [formData, setFormData] = useState({ route_name: '', departure_time: '', day_type: 'Weekday' })

    useEffect(() => {
        fetchItems()
    }, [])

    async function fetchItems() {
        const { data } = await supabase.from('schedules').select('*').order('departure_time', { ascending: true })
        if (data) setItems(data)
    }

    async function handleSubmit(e) {
        e.preventDefault()
        const { error } = await supabase.from('schedules').insert([formData])
        if (!error) {
            setShowModal(false)
            setFormData({ route_name: '', departure_time: '', day_type: 'Weekday' })
            fetchItems()
        } else {
            alert(error.message)
        }
    }

    async function deleteItem(id) {
        if (!confirm('Delete this schedule?')) return
        await supabase.from('schedules').delete().eq('id', id)
        fetchItems()
    }

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
                <h1>Bus Schedule</h1>
                <button className="btn btn-primary" onClick={() => setShowModal(true)}>+ Add Trip</button>
            </div>

            <div className="card">
                <table>
                    <thead>
                        <tr>
                            <th>Time</th>
                            <th>Route</th>
                            <th>Day Type</th>
                            <th>Status</th>
                            <th>Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        {items.map(item => (
                            <tr key={item.id}>
                                <td style={{ fontWeight: 'bold' }}>{item.departure_time.slice(0, 5)}</td>
                                <td>{item.route_name}</td>
                                <td>{item.day_type}</td>
                                <td>{item.status}</td>
                                <td>
                                    <button className="btn btn-danger" style={{ padding: '4px 8px', fontSize: '0.8rem' }} onClick={() => deleteItem(item.id)}>Remove</button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            {showModal && (
                <div className="modal-overlay" onClick={() => setShowModal(false)}>
                    <div className="modal" onClick={e => e.stopPropagation()}>
                        <h2>Add Schedule</h2>
                        <form onSubmit={handleSubmit}>
                            <select
                                value={formData.route_name}
                                onChange={e => setFormData({ ...formData, route_name: e.target.value })}
                                required
                            >
                                <option value="">Select Route</option>
                                <option value="Route A (Dorm -> Campus)">Route A (Dorm -&gt; Campus)</option>
                                <option value="Route B (Campus -> Hostel)">Route B (Campus -&gt; Hostel)</option>
                            </select>
                            <input
                                type="time"
                                value={formData.departure_time}
                                onChange={e => setFormData({ ...formData, departure_time: e.target.value })}
                                required
                            />
                            <select
                                value={formData.day_type}
                                onChange={e => setFormData({ ...formData, day_type: e.target.value })}
                            >
                                <option value="Weekday">Weekday</option>
                                <option value="Weekend">Weekend</option>
                            </select>
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
