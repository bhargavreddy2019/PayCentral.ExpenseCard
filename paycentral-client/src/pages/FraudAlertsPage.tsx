import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import apiClient from '../api/apiClient'
import { CheckCircle } from 'lucide-react'

interface FraudAlert {
  id: string
  cardNumber: string
  cardholderName: string
  alertType: string
  reason: string
  severity: number
  isResolved: boolean
  resolvedAt: string | null
  createdAt: string
}

const severityMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Low', color: 'bg-blue-500/20 text-blue-400' },
  2: { label: 'Medium', color: 'bg-yellow-500/20 text-yellow-400' },
  3: { label: 'High', color: 'bg-orange-500/20 text-orange-400' },
  4: { label: 'Critical', color: 'bg-red-500/20 text-red-400' }
}

export default function FraudAlertsPage() {
  const queryClient = useQueryClient()

  const { data: alerts, isLoading } = useQuery({
    queryKey: ['fraud-alerts-all'],
    queryFn: async () => {
      const { data } = await apiClient.get('/Fraud/alerts?pageSize=50')
      return data.data as FraudAlert[]
    }
  })

  const resolveMutation = useMutation({
    mutationFn: (alertId: string) => apiClient.put(`/Fraud/alerts/${alertId}/resolve`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['fraud-alerts-all'] })
  })

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold">Fraud Alerts</h2>
      <div className="bg-gray-800 rounded-xl overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-gray-700 text-gray-400 text-left">
              <th className="px-6 py-4">Cardholder</th>
              <th className="px-6 py-4">Alert Type</th>
              <th className="px-6 py-4">Reason</th>
              <th className="px-6 py-4">Severity</th>
              <th className="px-6 py-4">Time</th>
              <th className="px-6 py-4">Action</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <tr>
                <td colSpan={6} className="px-6 py-8 text-center text-gray-500">Loading...</td>
              </tr>
            ) : alerts?.map(alert => {
              const sev = severityMap[alert.severity]
              return (
                <tr key={alert.id}
                  className={`border-b border-gray-700/50 transition-colors
                    ${alert.isResolved ? 'opacity-50' : 'hover:bg-gray-700/30'}`}>
                  <td className="px-6 py-4">
                    <p className="font-medium">{alert.cardholderName}</p>
                    <p className="text-gray-500 text-xs font-mono">{alert.cardNumber}</p>
                  </td>
                  <td className="px-6 py-4 text-gray-300">
                    {alert.alertType.replace(/_/g, ' ')}
                  </td>
                  <td className="px-6 py-4 text-gray-400 max-w-xs truncate">{alert.reason}</td>
                  <td className="px-6 py-4">
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${sev.color}`}>
                      {sev.label}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-gray-400 text-xs">
                    {new Date(alert.createdAt).toLocaleString()}
                  </td>
                  <td className="px-6 py-4">
                    {!alert.isResolved ? (
                      <button
                        onClick={() => resolveMutation.mutate(alert.id)}
                        className="flex items-center gap-1 px-3 py-1.5 bg-green-500/20
                                   text-green-400 rounded-lg text-xs hover:bg-green-500/30 transition-colors"
                      >
                        <CheckCircle size={12} /> Resolve
                      </button>
                    ) : (
                      <span className="text-gray-500 text-xs">Resolved</span>
                    )}
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