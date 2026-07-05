import { type ReactNode } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { CreditCard, AlertTriangle, BarChart2, FileText, LogOut, Activity } from 'lucide-react'

export default function AdminLayout({ children }: { children: ReactNode }) {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  const navItems = [
    { to: '/admin', label: 'Dashboard', icon: Activity, end: true },
    { to: '/admin/cards', label: 'Cards', icon: CreditCard },
    { to: '/admin/fraud', label: 'Fraud Alerts', icon: AlertTriangle },
    { to: '/admin/transactions', label: 'Transactions', icon: BarChart2 },
    { to: '/admin/reports', label: 'Reports', icon: FileText },
  ]

  return (
    <div className="flex h-screen bg-gray-900 text-white">
      <aside className="w-64 bg-gray-800 flex flex-col">
        <div className="p-6 border-b border-gray-700">
          <h1 className="text-xl font-bold text-green-400">PayCentral</h1>
          <p className="text-xs text-gray-400 mt-1">Admin Portal</p>
        </div>
        <nav className="flex-1 p-4 space-y-1">
          {navItems.map(({ to, label, icon: Icon, end }) => (
            <NavLink
              key={to}
              to={to}
              end={end}
              className={({ isActive }) =>
                `flex items-center gap-3 px-4 py-3 rounded-lg text-sm
                 transition-colors ${isActive
                  ? 'bg-green-500/20 text-green-400'
                  : 'text-gray-400 hover:bg-gray-700 hover:text-white'}`
              }
            >
              <Icon size={18} />
              {label}
            </NavLink>
          ))}
        </nav>
        <div className="p-4 border-t border-gray-700">
          <div className="text-sm text-gray-300 mb-3">
            <p className="font-medium">{user?.fullName}</p>
            <p className="text-gray-500 text-xs">{user?.email}</p>
          </div>
          <button
            onClick={handleLogout}
            className="flex items-center gap-2 text-gray-400
                       hover:text-red-400 text-sm transition-colors"
          >
            <LogOut size={16} />
            Sign Out
          </button>
        </div>
      </aside>
      <main className="flex-1 overflow-auto p-6">
        {children}
      </main>
    </div>
  )
}