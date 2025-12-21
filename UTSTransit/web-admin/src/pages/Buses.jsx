
import { useEffect, useState } from 'react'
import { supabase } from '../supabaseClient'

export default function Buses() {
    const [buses, setBuses] = useState([])
    const [showModal, setShowModal] = useState(false)
    const [formData, setFormData] = useState({ plate_number: '', bus_name: '', capacity: 40 })

    useEffect(() => {
        fetchBuses()
    }, [])

    async function fetchBuses() {
        const { data } = await supabase.from('buses').select('*').order('bus_name', { ascending: true })
        if (data) setBuses(data)
    }

    async function handleSubmit(e) {
        e.preventDefault()
        const { error } = await supabase.from('buses').insert([formData])
        if (!error) {
            setShowModal(false)
            setFormData({ plate_number: '', bus_name: '', capacity: 40 })
            fetchBuses()
        } else {
            alert(error.message)
        }
    }

    async function deleteBus(id) {
        if (!confirm('Delete this bus?')) return
        await supabase.from('buses').delete().eq('id', id)
        fetchBuses()
    }

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
                <h1>Bus Management</h1>
                <button className="btn btn-primary" onClick={() => setShowModal(true)}>+ Add Bus</button>
            </div>

            <p style={{ color: '#666', marginBottom: '1rem' }}>
                Manage the buses available for drivers to select when starting a trip.
            </p>

            <div className="card">
                {buses.length === 0 ? (
                    <p style={{ color: '#888', textAlign: 'center', padding: '2rem' }}>No buses added yet. Click "+ Add Bus" to add one.</p>
                ) : (
                    <table>
                        <thead>
                            <tr>
                                <th>Bus Name</th>
                                <th>Plate Number</th>
                                <th>Capacity</th>
                                <th>Action</th>
                            </tr>
                        </thead>
                        <tbody>
                            {buses.map(bus => (
                                <tr key={bus.id}>
                                    <td style={{ fontWeight: 'bold' }}>{bus.bus_name}</td>
                                    <td>{bus.plate_number}</td>
                                    <td>{bus.capacity} seats</td>
                                    <td>
                                        <button className="btn btn-danger" style={{ padding: '4px 8px', fontSize: '0.8rem' }} onClick={() => deleteBus(bus.id)}>Remove</button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}
            </div>

            {showModal && (
                <div className="modal-overlay" onClick={() => setShowModal(false)}>
                    <div className="modal" onClick={e => e.stopPropagation()}>
                        <h2>Add New Bus</h2>
                        <form onSubmit={handleSubmit}>
                            <input
                                type="text"
                                placeholder="Bus Name (e.g., Bus A)"
                                value={formData.bus_name}
                                onChange={e => setFormData({ ...formData, bus_name: e.target.value })}
                                required
                            />
                            <input
                                type="text"
                                placeholder="Plate Number (e.g., QSK 1234)"
                                value={formData.plate_number}
                                onChange={e => setFormData({ ...formData, plate_number: e.target.value })}
                                required
                            />
                            <input
                                type="number"
                                placeholder="Capacity"
                                value={formData.capacity}
                                onChange={e => setFormData({ ...formData, capacity: parseInt(e.target.value) })}
                                min="1"
                                max="100"
                            />
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
