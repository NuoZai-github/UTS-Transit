
import { useEffect, useState } from 'react'
import { supabase } from '../supabaseClient'

export default function Users() {
    const [users, setUsers] = useState([])
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        fetchUsers()
    }, [])

    async function fetchUsers() {
        const { data, error } = await supabase
            .from('profiles')
            .select('*')
            .order('created_at', { ascending: false })

        if (data) setUsers(data)
        setLoading(false)
    }

    async function deleteUser(id) {
        if (!confirm('Are you sure you want to delete this user? This cannot be undone.')) return

        const { error } = await supabase
            .from('profiles')
            .delete()
            .eq('id', id)

        if (error) {
            alert('Error deleting user: ' + error.message)
        } else {
            fetchUsers()
        }
    }

    if (loading) return <div>Loading...</div>

    return (
        <div>
            <h1>User Management</h1>
            <div className="card">
                <table>
                    <thead>
                        <tr>
                            <th>Avatar</th>
                            <th>Email</th>
                            <th>Role</th>
                            <th>Details</th>
                            <th>Joined</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {users.map(user => (
                            <tr key={user.id}>
                                <td>
                                    {user.avatar_url ? (
                                        <img src={user.avatar_url} alt="Avatar" style={{ width: '40px', height: '40px', borderRadius: '50%', objectFit: 'cover' }} />
                                    ) : (
                                        <div style={{ width: '40px', height: '40px', borderRadius: '50%', background: '#eee', display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#888' }}>
                                            ?
                                        </div>
                                    )}
                                </td>
                                <td>{user.email}</td>
                                <td>
                                    <span style={{
                                        padding: '4px 8px',
                                        borderRadius: '4px',
                                        background: user.role === 'driver' ? '#E0F2FE' : '#F1F5F9',
                                        color: user.role === 'driver' ? '#0284C7' : '#64748B',
                                        fontWeight: 'bold',
                                        fontSize: '0.8rem'
                                    }}>
                                        {user.role.toUpperCase()}
                                    </span>
                                </td>
                                <td>
                                    {user.role === 'student' && user.student_id && (
                                        <div><small style={{ color: 'var(--text-secondary)' }}>ID:</small> {user.student_id}</div>
                                    )}
                                    {user.role === 'driver' && user.ic_number && (
                                        <div><small style={{ color: 'var(--text-secondary)' }}>IC:</small> {user.ic_number}</div>
                                    )}
                                </td>
                                <td>{new Date(user.created_at).toLocaleDateString()}</td>
                                <td>
                                    <button className="btn btn-danger" onClick={() => deleteUser(user.id)}>Delete</button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    )
}
