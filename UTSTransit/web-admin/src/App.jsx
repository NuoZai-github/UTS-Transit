
import { BrowserRouter as Router, Routes, Route, NavLink } from 'react-router-dom'
import Dashboard from './pages/Dashboard'
import Users from './pages/Users'
import MapPage from './pages/MapPage'
import Announcements from './pages/Announcements'
import Schedule from './pages/Schedule'

function App() {
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
          </nav>
        </aside>
        <main className="content">
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/users" element={<Users />} />
            <Route path="/map" element={<MapPage />} />
            <Route path="/announcements" element={<Announcements />} />
            <Route path="/schedule" element={<Schedule />} />
          </Routes>
        </main>
      </div>
    </Router>
  )
}

export default App
