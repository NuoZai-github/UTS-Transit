import { BrowserRouter as Router, Routes, Route, NavLink, Navigate } from 'react-router-dom'
import { useEffect, useState } from 'react'
import { supabase } from './supabaseClient'
import Login from './pages/Login'
import Signup from './pages/Signup'
import Dashboard from './pages/Dashboard'
import Users from './pages/Users'
import MapPage from './pages/MapPage'
import Announcements from './pages/Announcements'
import Schedule from './pages/Schedule'
import Buses from './pages/Buses'
import Tracking from './pages/Tracking'

function App() {
  const [session, setSession] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    supabase.auth.getSession().then(({ data: { session } }) => {
      setSession(session)
      setLoading(false)
    })

    const {
      data: { subscription },
    } = supabase.auth.onAuthStateChange((_event, session) => {
      setSession(session)
    })

    return () => subscription.unsubscribe()
  }, [])

  async function handleLogout() {
    await supabase.auth.signOut()
  }

  if (loading) return null

  if (!session) {
    return (
      <Router>
        <Routes>
          <Route path="/signup" element={<Signup />} />
          <Route path="*" element={<Login />} />
        </Routes>
      </Router>
    )
  }

  return (
    <Router>
      <div className="layout">
        <aside className="sidebar">
          <div className="sidebar-title">UTS Admin</div>
          <nav>
            <NavLink to="/" className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>
              📊 Dashboard
            </NavLink>
            <NavLink to="/users" className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>
              👥 Users
            </NavLink>
            <NavLink to="/map" className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>
              🗺️ Live Map
            </NavLink>
            <NavLink to="/announcements" className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>
              📢 Announcements
            </NavLink>
            <NavLink to="/schedule" className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>
              📅 Schedule
            </NavLink>
            <NavLink to="/tracking" className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>
              📍 Tracking
            </NavLink>
            <NavLink to="/buses" className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>
              🚌 Buses
            </NavLink>
          </nav>
          <div style={{ marginTop: 'auto' }}>
            <button onClick={handleLogout} className="nav-link" style={{ background: 'none', border: 'none', width: '100%', cursor: 'pointer', textAlign: 'left' }}>
              🚪 Logout
            </button>
          </div>
        </aside>
        <main className="content">
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/users" element={<Users />} />
            <Route path="/map" element={<MapPage />} />
            <Route path="/announcements" element={<Announcements />} />
            <Route path="/schedule" element={<Schedule />} />
            <Route path="/tracking" element={<Tracking />} />
            <Route path="/buses" element={<Buses />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </main>
      </div>
    </Router>
  )
}

export default App
