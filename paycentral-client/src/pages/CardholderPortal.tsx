import { useQuery } from '@tanstack/react-query'
import apiClient from '../api/apiClient'
import { useAuth } from '../context/AuthContext'
import { CreditCard, LogOut } from 'lucide-react'
import { useNavigate } from 'react-router-dom'

interface Card {
  id: string
  maskedCardNumber: string
  status: number
  balance: number
  availableBalance: number
  currency: string
}

interface Transaction {
  id: string
  referenceNumber: string
  type: number
  status: number
  amount: number
  balanceAfter: number
  merchantName: string | null
  description: string | null
  createdAt: string
}

const typeMap: Record<number, string> = {
  1: 'Purchase', 2: 'Reversal', 3: 'Fee',
  4: 'Balance Enquiry', 5: 'Refund', 6: 'Load Funds', 7: 'Debit'
}

const cardStatusMap: Record<number, { label: string; color: string }> = {
  0: { label: 'Pending', color: 'text-yellow-400' },
  1: { label: 'Active', color: 'text-green-400' },
  2: { label: 'Blocked', color: 'text-red-400' },
  3: { label: 'Suspended', color: 'text-orange-400' },
  4: { label: 'Closed', color: 'text-gray-400' }
}

export default function CardholderPortal() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const { data: cards } = useQuery({
    queryKey: ['my-cards'],
    queryFn: async () => {
      const tokenPayload = JSON.parse(atob(localStorage.getItem('accessToken')!.split('.')[1]))
      const userId = tokenPayload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']
      const { data } = await apiClient.get(`/Cards/user/${userId}`)
      return data.data as Card[]
    }
  })

  const firstCard = cards?.[0]

  const { data: transactions } = useQuery({
    queryKey: ['my-transactions', firstCard?.id],
    enabled: !!firstCard?.id,
    queryFn: async () => {
      const { data } = await apiClient.get(`/Transactions/card/${firstCard!.id}?pageSize=10`)
      return data.data as Transaction[]
    }
  })

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="min-h-screen bg-gray-900 text-white">
      <header className="bg-gray-800 border-b border-gray-700 px-6 py-4
                         flex items-center justify-between">
        <h1 className="text-xl font-bold text-green-400">PayCentral</h1>
        <div className="flex items-center gap-4">
          <span className="text-gray-400 text-sm">{user?.fullName}</span>
          <button
            onClick={handleLogout}
            className="flex items-center gap-2 text-gray-400
                       hover:text-red-400 text-sm transition-colors"
          >
            <LogOut size={16} /> Sign Out
          </button>
        </div>
      </header>
      <div className="max-w-2xl mx-auto p-6 space-y-6">
        {firstCard && (
          <div className="bg-gradient-to-br from-green-600 to-green-800 rounded-2xl p-6 shadow-xl">
            <div className="flex items-center justify-between mb-8">
              <span className="text-green-200 text-sm font-medium">Corporate Expense Card</span>
              <CreditCard size={24} className="text-green-200" />
            </div>
            <p className="font-mono text-xl tracking-widest mb-6">{firstCard.maskedCardNumber}</p>
            <div className="flex items-end justify-between">
              <div>
                <p className="text-green-300 text-xs mb-1">Available Balance</p>
                <p className="text-3xl font-bold">R{firstCard.availableBalance.toLocaleString()}</p>
              </div>
              <span className={`text-sm font-medium ${cardStatusMap[firstCard.status]?.color}`}>
                {cardStatusMap[firstCard.status]?.label}
              </span>
            </div>
          </div>
        )}
        <div className="bg-gray-800 rounded-xl p-6">
          <h3 className="font-semibold mb-4">Recent Transactions</h3>
          <div className="space-y-3">
            {transactions?.map(tx => (
              <div key={tx.id}
                className="flex items-center justify-between py-3 border-b border-gray-700/50 last:border-0">
                <div>
                  <p className="text-sm font-medium">
                    {tx.merchantName || tx.description || typeMap[tx.type]}
                  </p>
                  <p className="text-xs text-gray-500 mt-0.5">
                    {typeMap[tx.type]} • {new Date(tx.createdAt).toLocaleDateString()}
                  </p>
                </div>
                <div className="text-right">
                  <p className={`text-sm font-semibold ${tx.type === 6 || tx.type === 5 ? 'text-green-400' : 'text-red-400'}`}>
                    {tx.type === 6 || tx.type === 5 ? '+' : '-'}R{tx.amount.toLocaleString()}
                  </p>
                  <p className="text-xs text-gray-500">Bal: R{tx.balanceAfter.toLocaleString()}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  )
}