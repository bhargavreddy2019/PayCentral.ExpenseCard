import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { AuthProvider, useAuth } from './context/AuthContext'
import LoginPage from './pages/LoginPage'
import AdminDashboard from './pages/AdminDashboard'
import CardsPage from './pages/CardsPage'
import FraudAlertsPage from './pages/FraudAlertsPage'
import CardholderPortal from './pages/CardholderPortal'
import AdminLayout from './components/AdminLayout'

const queryClient = new QueryClient()

function ProtectedAdmin({ children }: { children: React.ReactNode }) {
  const { user, isAdmin } = useAuth()
  if (!user) return <Navigate to="/login" />
  if (!isAdmin) return <Navigate to="/cardholder" />
  return <AdminLayout>{children}</AdminLayout>
}

function ProtectedCardholder({ children }: { children: React.ReactNode }) {
  const { user } = useAuth()
  if (!user) return <Navigate to="/login" />
  return <>{children}</>
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/admin" element={<ProtectedAdmin><AdminDashboard /></ProtectedAdmin>} />
      <Route path="/admin/cards" element={<ProtectedAdmin><CardsPage /></ProtectedAdmin>} />
      <Route path="/admin/fraud" element={<ProtectedAdmin><FraudAlertsPage /></ProtectedAdmin>} />
      <Route path="/cardholder" element={<ProtectedCardholder><CardholderPortal /></ProtectedCardholder>} />
      <Route path="/" element={<Navigate to="/login" />} />
    </Routes>
  )
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <BrowserRouter>
          <AppRoutes />
        </BrowserRouter>
      </AuthProvider>
    </QueryClientProvider>
  )
}