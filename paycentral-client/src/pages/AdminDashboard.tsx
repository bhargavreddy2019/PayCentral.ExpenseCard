import { useEffect, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import * as signalR from '@microsoft/signalr'
import apiClient from '../api/apiClient'
import { CreditCard, AlertTriangle, Activity, TrendingUp } from 'lucide-react'

interface FraudAlert {
  id: string
  cardNumber: string
  cardholderName: string
  alertType: string
  reason: string
  severity: number
  createdAt: string
}

interface DailySummary {
  totalTransactions: number
  totalAmount: number
  totalPurchases: number
  fraudAlertsRaised: number
  newCardsIssued: number
}

export default function AdminDashboard() {
  const [liveAlerts, setLiveAlerts] = useState<FraudAlert[]>([])

  const { data: summary } = useQuery({
    queryKey: ['daily-summary'],
    queryFn: async () => {
      const { data } = await apiClient.get('/Reports/daily-summary')
      return data.data as DailySummary
    }
  })

  const { data: fraudAlerts } = useQuery({
    queryKey: ['fraud-alerts'],
    queryFn: async () => {
      const { data } = await apiClient.get('/Fraud/alerts?isResolved=false')
      return data.data as FraudAlert[]
    }
  })

  useEffect(() => {
    const token = localStorage.getItem('accessToken')
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/fraud', { accessTokenFactory: () => token || '' })
      .withAutomaticReconnect()
      .build()

    connection.on('ReceiveFraudAlert', (alert: FraudAlert) => {
      setLiveAlerts(prev => [alert, ...prev])
    })

    connection.start().catch(console.error)
    return () => { connection.stop() }
  }, [])

  const allAlerts = [...liveAlerts, ...(fraudAlerts || [])]
    .filter((alert, index, self) =>
      index === self.findIndex(a => a.id === alert.id))

  const severityLabel = (s: number) => {
    const map: Record<number, { label: string; color: string }> = {
      1: { label: 'Low', color: 'text-blue-400' },
      2: { label: 'Medium', color: 'text-yellow-400' },
      3: { label: 'High', color: 'text-orange-400' },
      4: { label: 'Critical', color: 'text-red-400' }
    }
    return map[s] || { label: 'Unknown', color: 'text-gray-400' }
  }

  const stats = [
    { label: 'Transactions Today', value: summary?.totalTransactions ?? 0, icon: Activity, color: 'text-blue-400' },
    { label: 'Total Amount', value: `R${(summary?.totalAmount ?? 0).toLocaleString()}`, icon: TrendingUp, color: 'text-green-400' },
    { label: 'Fraud Alerts', value: allAlerts.length, icon: AlertTriangle, color: 'text-red-400' },
    { label: 'New Cards', value: summary?.newCardsIssued ?? 0, icon: CreditCard, color: 'text-purple-400' }
  ]

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold">Dashboard</h2>
      <div className="grid grid-cols-4 gap-4">
        {stats.map(({ label, value, icon: Icon, color }) => (
          <div key={label} className="bg-gray-800 rounded-xl p-5">
            <div className="flex items-center justify-between mb-3">
              <span className="text-gray-400 text-sm">{label}</span>
              <Icon size={20} className={color} />
            </div>
            <p className={`text-2xl font-bold ${color}`}>{value}</p>
          </div>
        ))}
      </div>
      <div className="bg-gray-800 rounded-xl p-6">
        <div className="flex items-center gap-2 mb-4">
          <AlertTriangle size={20} className="text-red-400" />
          <h3 className="text-lg font-semibold">Live Fraud Alerts</h3>
          {liveAlerts.length > 0 && (
            <span className="bg-red-500 text-white text-xs px-2 py-0.5 rounded-full animate-pulse">
              {liveAlerts.length} NEW
            </span>
          )}
        </div>
        {allAlerts.length === 0 ? (
          <p className="text-gray-500 text-sm">No active fraud alerts</p>
        ) : (
          <div className="space-y-3">
            {allAlerts.slice(0, 10).map(alert => {
              const sev = severityLabel(alert.severity)
              return (
                <div key={alert.id}
                  className="flex items-center justify-between bg-gray-700/50 rounded-lg px-4 py-3">
                  <div>
                    <p className="text-sm font-medium">
                      {alert.cardholderName}
                      <span className="text-gray-400 ml-2 font-normal">{alert.cardNumber}</span>
                    </p>
                    <p className="text-xs text-gray-400 mt-0.5">{alert.reason}</p>
                  </div>
                  <div className="text-right">
                    <p className={`text-sm font-semibold ${sev.color}`}>{sev.label}</p>
                    <p className="text-xs text-gray-500">{alert.alertType}</p>
                  </div>
                </div>
              )
            })}
          </div>
        )}
      </div>
    </div>
  )
}