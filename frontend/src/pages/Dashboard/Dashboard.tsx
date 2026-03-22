import { useEffect, useState } from 'react'
import { dashboardApi } from '@/api/client'
import type { DashboardData } from '@/api/types'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { formatDate } from '@/lib/utils'

export function Dashboard() {
  const [data, setData] = useState<DashboardData | null>(null)

  useEffect(() => {
    dashboardApi.get().then(setData)
    const interval = setInterval(() => dashboardApi.get().then(setData), 10000)
    return () => clearInterval(interval)
  }, [])

  if (!data) return <div className="text-muted-foreground">Loading...</div>

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold">Dashboard</h2>

      <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
        {Object.entries(data.counts).map(([key, val]) => (
          <Card key={key}>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground capitalize">
                {key.replace(/([A-Z])/g, ' $1')}
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{val}</div>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle>Recent Adapter Logs</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            {data.recentAdapterLogs.length === 0 && (
              <p className="text-sm text-muted-foreground">No logs yet</p>
            )}
            {data.recentAdapterLogs.map(log => (
              <div key={log.id} className="flex items-center justify-between text-sm border-b pb-2">
                <span className="font-medium">{log.adapter?.name ?? log.adapterId}</span>
                <div className="flex items-center gap-2">
                  <Badge variant={log.triggeredAnyRule ? 'success' : 'secondary'}>
                    {log.triggeredAnyRule ? 'Triggered' : 'No trigger'}
                  </Badge>
                  <span className="text-muted-foreground text-xs">{formatDate(log.receivedAt)}</span>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Recent Action Logs</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            {data.recentActionLogs.length === 0 && (
              <p className="text-sm text-muted-foreground">No logs yet</p>
            )}
            {data.recentActionLogs.map(log => (
              <div key={log.id} className="flex items-center justify-between text-sm border-b pb-2">
                <span className="font-medium">{log.lever?.name ?? log.leverId}</span>
                <div className="flex items-center gap-2">
                  <Badge variant={log.success ? 'success' : 'destructive'}>
                    {log.success ? 'OK' : 'Failed'}
                  </Badge>
                  <Badge variant="outline">{log.source}</Badge>
                  <span className="text-muted-foreground text-xs">{formatDate(log.calledAt)}</span>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
