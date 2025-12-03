
import { useEffect, useState } from 'react'
import { supabase } from '../supabaseClient'

export default function Announcements() {
    const [items, setItems] = useState([])
    const [showModal, setShowModal] = useState(false)
    const [formData, setFormData] = useState({ title: '', content: '', is_urgent: false })

    useEffect(() => {
        fetchItems()
    }, [])

    async function fetchItems() {
        const { data } = await supabase.from('announcements').select('*').order('created_at', { ascending: false })
        if (data) setItems(data)
    }

    async function handleSubmit(e) {
        e.preventDefault()
        const { error } = await supabase.from('announcements').insert([formData])
        if (!error) {
            setShowModal(false)
            setFormData({ title: '', content: '', is_urgent: false })
            fetchItems()
        } else {
            alert(error.message)
        }
    }

    async function deleteItem(id) {
        if (!confirm('Delete this announcement?')) return
        await supabase.from('announcements').delete().eq('id', id)
        fetchItems()
    }

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
                <h1>Announcements</h1>
                <button className="btn btn-primary" onClick={() => setShowModal(true)}>+ New Announcement</button>
            </div>

            <div className="grid-3">
                {items.map(item => (
                    <div key={item.id} className="card" style={{ borderLeft: item.is_urgent ? '4px solid var(--danger)' : '4px solid var(--primary)' }}>
                        <h3>{item.title}</h3>
                        <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{new Date(item.created_at).toLocaleDateString()}</p>
                        <p>{item.content}</p>
                        <button className="btn btn-danger" style={{ marginTop: '1rem', fontSize: '0.8rem' }} onClick={() => deleteItem(item.id)}>Delete</button>
                    </div>
                ))}
            </div>

            {showModal && (
                <div className="modal-overlay" onClick={() => setShowModal(false)}>
                    <div className="modal" onClick={e => e.stopPropagation()}>
                        <h2>New Announcement</h2>
                        <form onSubmit={handleSubmit}>
                            <input
                                placeholder="Title"
                                value={formData.title}
                                onChange={e => setFormData({ ...formData, title: e.target.value })}
                                required
                            />
                            <textarea
                                placeholder="Content"
                                value={formData.content}
                                onChange={e => setFormData({ ...formData, content: e.target.value })}
                                required
                                rows="4"
                            />
                            <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '1rem' }}>
                                <input
                                    type="checkbox"
                                    checked={formData.is_urgent}
                                    onChange={e => setFormData({ ...formData, is_urgent: e.target.checked })}
                                    style={{ width: 'auto', margin: 0 }}
                                />
                                Urgent?
                            </label>
                            <div style={{ display: 'flex', gap: '1rem', justifyContent: 'flex-end' }}>
                                <button type="button" className="btn" onClick={() => setShowModal(false)}>Cancel</button>
                                <button type="submit" className="btn btn-primary">Post</button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    )
}
