
import { useEffect, useState } from 'react'
import { supabase } from '../supabaseClient'

export default function Dashboard() {
    const [stats, setStats] = useState({
        students: 0,
        drivers: 0,
        activeBuses: 0
    })

    useEffect(() => {
        fetchStats()

        // Subscribe to active_trips changes
        const channel = supabase
            .channel('dashboard_stats')
            .on('postgres_changes', { event: '*', schema: 'public', table: 'active_trips' }, () => {
                fetchStats()
            })
            .subscribe()

        return () => {
            supabase.removeChannel(channel)
        }
    }, [])

    async function fetchStats() {
        // Count students
        const { count: studentCount } = await supabase
            .from('profiles')
            .select('*', { count: 'exact', head: true })
            .eq('role', 'student')

        // Count drivers
        const { count: driverCount } = await supabase
            .from('profiles')
            .select('*', { count: 'exact', head: true })
            .eq('role', 'driver')

        // Count active buses
        const { count: busCount } = await supabase
            .from('active_trips')
            .select('*', { count: 'exact', head: true })

        setStats({
            students: studentCount || 0,
            drivers: driverCount || 0,
            activeBuses: busCount || 0
        })
    }

    return (
        <div>
            <h1>Dashboard</h1>
            <div className="grid-3">
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
            </div>
        </div>
    )
}
