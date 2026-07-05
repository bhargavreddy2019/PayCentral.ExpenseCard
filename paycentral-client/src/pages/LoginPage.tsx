import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import apiClient from '../api/apiClient'
import { useAuth } from '../context/AuthContext'

export default function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const { login } = useAuth()
  const navigate = useNavigate()

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      const { data } = await apiClient.post('/Auth/login', { email, password })
      login({
        email: data.email,
        fullName: data.fullName,
        role: data.role,
        accessToken: data.accessToken,
        refreshToken: data.refreshToken
      })
      navigate(data.role === 'Administrator' ? '/admin' : '/cardholder')
    } catch {
      setError('Invalid email or password')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-gray-900 flex items-center justify-center">
      <div className="bg-gray-800 p-8 rounded-xl shadow-2xl w-full max-w-md">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-green-400">PayCentral</h1>
          <p className="text-gray-400 mt-1">Corporate Expense Card Platform</p>
        </div>
        <form onSubmit={handleLogin} className="space-y-5">
          <div>
            <label className="block text-sm text-gray-300 mb-1">
              Email Address
            </label>
            <input
              type="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              className="w-full bg-gray-700 text-white rounded-lg px-4 py-3
                         border border-gray-600 focus:outline-none
                         focus:border-green-400"
              placeholder="admin@paycentral.co.za"
              required
            />
          </div>
          <div>
            <label className="block text-sm text-gray-300 mb-1">
              Password
            </label>
            <input
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              className="w-full bg-gray-700 text-white rounded-lg px-4 py-3
                         border border-gray-600 focus:outline-none
                         focus:border-green-400"
              placeholder="••••••••"
              required
            />
          </div>
          {error && (
            <div className="bg-red-900/50 border border-red-500 text-red-300
                           rounded-lg px-4 py-3 text-sm">
              {error}
            </div>
          )}
          <button
            type="submit"
            disabled={loading}
            className="w-full bg-green-500 hover:bg-green-400 
                       disabled:bg-green-800 text-white font-semibold 
                       rounded-lg py-3 transition-colors"
          >
            {loading ? 'Signing in...' : 'Sign In'}
          </button>
        </form>
        <div className="mt-6 p-4 bg-gray-700/50 rounded-lg text-xs text-gray-400">
          <p className="font-semibold text-gray-300 mb-2">Test Credentials</p>
          <p>Admin: admin@paycentral.co.za / Admin@123</p>
          <p>Cardholder: john.doe@paycentral.co.za / Cardholder@123</p>
        </div>
      </div>
    </div>
  )
}