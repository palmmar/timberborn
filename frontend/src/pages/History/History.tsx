import { useEffect, useState, useCallback, useRef } from 'react'
import { logsApi } from '@/api/client'
import type { AdapterLog, ActionLog } from '@/api/types'
import { useLogStream } from '@/api/useLogStream'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Dialog } from '@/components/ui/dialog'
import { Switch } from '@/components/ui/switch'
import { Label } from '@/components/ui/label'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { Trash2, RefreshCw } from 'lucide-react'
import { formatDate } from '@/lib/utils'

type Tab = 'adapter' | 'action'

export function History() {
  const [tab, setTab] = useState<Tab>('adapter')
  const [adapterLogs, setAdapterLogs] = useState<AdapterLog[]>([])
  const [actionLogs, setActionLogs] = useState<ActionLog[]>([])
  const [adapterTotal, setAdapterTotal] = useState(0)
  const [actionTotal, setActionTotal] = useState(0)
  const [adapterPage, setAdapterPage] = useState(1)
  const [actionPage, setActionPage] = useState(1)
  const [live, setLive] = useState(true)
  const [selectedLog, setSelectedLog] = useState<AdapterLog | ActionLog | null>(null)
  const [confirmPurge, setConfirmPurge] = useState<Tab | null>(null)
  const PAGE_SIZE = 20

  const loadAdapterLogs = useCallback(async (page = 1) => {
    const r = await logsApi.adapterLogs({ page, pageSize: PAGE_SIZE })
    setAdapterLogs(r.items); setAdapterTotal(r.total)
  }, [])

  const loadActionLogs = useCallback(async (page = 1) => {
    const r = await logsApi.actionLogs({ page, pageSize: PAGE_SIZE })
    setActionLogs(r.items); setActionTotal(r.total)
  }, [])

  useEffect(() => { void loadAdapterLogs(adapterPage) }, [loadAdapterLogs, adapterPage])
  useEffect(() => { void loadActionLogs(actionPage) }, [loadActionLogs, actionPage])

  const onEventRef = useRef((e: { type: string; data: AdapterLog | ActionLog }) => {
    if (e.type === 'adapter_log') {
      setAdapterLogs(prev => [e.data as AdapterLog, ...prev].slice(0, PAGE_SIZE))
      setAdapterTotal(t => t + 1)
    } else if (e.type === 'action_log') {
      setActionLogs(prev => [e.data as ActionLog, ...prev].slice(0, PAGE_SIZE))
      setActionTotal(t => t + 1)
    }
  })
  onEventRef.current = (e) => {
    if (e.type === 'adapter_log') {
      setAdapterLogs(prev => [e.data as AdapterLog, ...prev].slice(0, PAGE_SIZE))
      setAdapterTotal(t => t + 1)
    } else if (e.type === 'action_log') {
      setActionLogs(prev => [e.data as ActionLog, ...prev].slice(0, PAGE_SIZE))
      setActionTotal(t => t + 1)
    }
  }

  useLogStream(useCallback((e) => onEventRef.current(e), []), live)

  const purge = async () => {
    if (confirmPurge === 'adapter') await logsApi.purgeAdapterLogs()
    else await logsApi.purgeActionLogs()
    setConfirmPurge(null)
    await loadAdapterLogs(1)
    await loadActionLogs(1)
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">History</h2>
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2">
            <Switch checked={live} onCheckedChange={setLive} />
            <Label>{live ? 'Live' : 'Paused'}</Label>
          </div>
          <Button variant="outline" size="sm" onClick={() => tab === 'adapter' ? loadAdapterLogs(adapterPage) : loadActionLogs(actionPage)}>
            <RefreshCw size={14} className="mr-1" />Refresh
          </Button>
          <Button variant="outline" size="sm" onClick={() => setConfirmPurge(tab)}>
            <Trash2 size={14} className="mr-1" />Purge
          </Button>
        </div>
      </div>

      <div className="flex gap-1 border-b">
        {(['adapter', 'action'] as Tab[]).map(t => (
          <button
            key={t}
            onClick={() => setTab(t)}
            className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${tab === t ? 'border-primary text-primary' : 'border-transparent text-muted-foreground hover:text-foreground'}`}
          >
            {t === 'adapter' ? `Adapter Logs (${adapterTotal})` : `Action Logs (${actionTotal})`}
          </button>
        ))}
      </div>

      {tab === 'adapter' && (
        <>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Adapter</TableHead>
                <TableHead>Received</TableHead>
                <TableHead>State</TableHead>
                <TableHead>Triggered</TableHead>
                <TableHead>Payload</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {adapterLogs.map(log => (
                <TableRow key={log.id} className="cursor-pointer" onClick={() => setSelectedLog(log)}>
                  <TableCell>{log.adapter?.name ?? log.adapterId}</TableCell>
                  <TableCell className="text-xs text-muted-foreground">{formatDate(log.receivedAt)}</TableCell>
                  <TableCell>{log.state ? <Badge variant={log.state === 'on' ? 'success' : 'secondary'}>{log.state}</Badge> : <span className="text-muted-foreground">—</span>}</TableCell>
                  <TableCell><Badge variant={log.triggeredAnyRule ? 'success' : 'secondary'}>{log.triggeredAnyRule ? 'Yes' : 'No'}</Badge></TableCell>
                  <TableCell className="max-w-xs truncate font-mono text-xs text-muted-foreground">{log.rawPayload}</TableCell>
                </TableRow>
              ))}
              {adapterLogs.length === 0 && <TableRow><TableCell colSpan={5} className="text-center text-muted-foreground">No logs</TableCell></TableRow>}
            </TableBody>
          </Table>
          <Pagination page={adapterPage} total={adapterTotal} pageSize={PAGE_SIZE} onPage={p => { setAdapterPage(p); void loadAdapterLogs(p) }} />
        </>
      )}

      {tab === 'action' && (
        <>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Lever</TableHead>
                <TableHead>Called</TableHead>
                <TableHead>Source</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Result</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {actionLogs.map(log => (
                <TableRow key={log.id} className="cursor-pointer" onClick={() => setSelectedLog(log)}>
                  <TableCell>{log.lever?.name ?? log.leverId}</TableCell>
                  <TableCell className="text-xs text-muted-foreground">{formatDate(log.calledAt)}</TableCell>
                  <TableCell><Badge variant="outline">{log.source}</Badge></TableCell>
                  <TableCell>{log.responseStatusCode ?? '—'}</TableCell>
                  <TableCell><Badge variant={log.success ? 'success' : 'destructive'}>{log.success ? 'OK' : 'Failed'}</Badge></TableCell>
                </TableRow>
              ))}
              {actionLogs.length === 0 && <TableRow><TableCell colSpan={5} className="text-center text-muted-foreground">No logs</TableCell></TableRow>}
            </TableBody>
          </Table>
          <Pagination page={actionPage} total={actionTotal} pageSize={PAGE_SIZE} onPage={p => { setActionPage(p); void loadActionLogs(p) }} />
        </>
      )}

      <Dialog open={!!selectedLog} onClose={() => setSelectedLog(null)} title="Log Detail" className="max-w-2xl">
        {selectedLog && (
          <pre className="text-xs bg-muted rounded p-3 overflow-auto max-h-96 whitespace-pre-wrap">
            {JSON.stringify(selectedLog, null, 2)}
          </pre>
        )}
      </Dialog>

      <Dialog open={!!confirmPurge} onClose={() => setConfirmPurge(null)} title="Purge Logs">
        <p className="text-sm mb-4">Delete all {confirmPurge} logs? This cannot be undone.</p>
        <div className="flex justify-end gap-2">
          <Button variant="outline" onClick={() => setConfirmPurge(null)}>Cancel</Button>
          <Button variant="destructive" onClick={purge}>Purge</Button>
        </div>
      </Dialog>
    </div>
  )
}

function Pagination({ page, total, pageSize, onPage }: { page: number; total: number; pageSize: number; onPage: (p: number) => void }) {
  const totalPages = Math.ceil(total / pageSize)
  if (totalPages <= 1) return null
  return (
    <div className="flex items-center justify-center gap-2 text-sm">
      <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => onPage(page - 1)}>Previous</Button>
      <span className="text-muted-foreground">Page {page} of {totalPages}</span>
      <Button variant="outline" size="sm" disabled={page >= totalPages} onClick={() => onPage(page + 1)}>Next</Button>
    </div>
  )
}
