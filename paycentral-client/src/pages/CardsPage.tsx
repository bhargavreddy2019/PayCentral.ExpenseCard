import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import apiClient from '../api/apiClient'
import { Search, Lock, Unlock } from 'lucide-react'

interface Card {
  id: string
  maskedCardNumber: string
  cardholderName: string
  email: string
  status: number
  balance: number
  availableBalance: number
  currency: string
}

const statusMap: Record<number, { label: string; color: string }> = {
  0: { label: 'Pending', color: 'bg-yellow-500/20 text-yellow-400' },
  1: { label: 'Active', color: 'bg-green-500/20 text-green-400' },
  2: { label: 'Blocked', color: 'bg-red-500/20 text-red-400' },
  3: { label: 'Suspended', color: 'bg-orange-500/20 text-orange-400' },
  4: { label: 'Closed', color: 'bg-gray-500/20 text-gray-400' }
}

export default function CardsPage() {
  const [search, setSearch] = useState('')
  const queryClient = useQueryClient()

  const { data: cards, isLoading } = useQuery({
    queryKey: ['cards', search],
    queryFn: async () => {
      const { data } = await apiClient.get(`/Cards?searchTerm=${search}&pageSize=20`)
      return data.data as Card[]
    }
  })

  const blockMutation = useMutation({
    mutationFn: (cardId: string) =>
      apiClient.put(`/Cards/${cardId}/block`, { reason: 'Blocked by administrator' }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cards'] })
  })

  const unblockMutation = useMutation({
    mutationFn: (cardId: string) => apiClient.put(`/Cards/${cardId}/unblock`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cards'] })
  })

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold">Cards</h2>
      <div className="relative">
        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
        <input
          value={search}
          onChange={e => setSearch(e.target.value)}
          placeholder="Search by name, email or card number..."
          className="w-full bg-gray-800 text-white rounded-lg pl-9 pr-4 py-3
                     border border-gray-700 focus:outline-none focus:border-green-400 text-sm"
        />
      </div>
      <div className="bg-gray-800 rounded-xl overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-gray-700 text-gray-400 text-left">
              <th className="px-6 py-4">Card Number</th>
              <th className="px-6 py-4">Cardholder</th>
              <th className="px-6 py-4">Status</th>
              <th className="px-6 py-4">Balance</th>
              <th className="px-6 py-4">Actions</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <tr>
                <td colSpan={5} className="px-6 py-8 text-center text-gray-500">Loading...</td>
              </tr>
            ) : cards?.map(card => {
              const status = statusMap[card.status]
              return (
                <tr key={card.id}
                  className="border-b border-gray-700/50 hover:bg-gray-700/30 transition-colors">
                  <td className="px-6 py-4 font-mono text-green-400">{card.maskedCardNumber}</td>
                  <td className="px-6 py-4">
                    <p className="font-medium">{card.cardholderName}</p>
                    <p className="text-gray-500 text-xs">{card.email}</p>
                  </td>
                  <td className="px-6 py-4">
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${status.color}`}>
                      {status.label}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <p className="font-medium">R{card.availableBalance.toLocaleString()}</p>
                    <p className="text-gray-500 text-xs">Balance: R{card.balance.toLocaleString()}</p>
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-2">
                      {card.status === 1 && (
                        <button
                          onClick={() => blockMutation.mutate(card.id)}
                          className="flex items-center gap-1 px-3 py-1.5 bg-red-500/20
                                     text-red-400 rounded-lg text-xs hover:bg-red-500/30 transition-colors"
                        >
                          <Lock size={12} /> Block
                        </button>
                      )}
                      {card.status === 2 && (
                        <button
                          onClick={() => unblockMutation.mutate(card.id)}
                          className="flex items-center gap-1 px-3 py-1.5 bg-green-500/20
                                     text-green-400 rounded-lg text-xs hover:bg-green-500/30 transition-colors"
                        >
                          <Unlock size={12} /> Unblock
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>
    </div>
  )
}