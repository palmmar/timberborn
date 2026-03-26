import { useEffect, useState, useCallback, useRef } from 'react'
import { adaptersApi } from '@/api/client'
import type { Adapter, AdapterLog } from '@/api/types'
import { useLogStream } from '@/api/useLogStream'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Dialog } from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Switch } from '@/components/ui/switch'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { Plus, Pencil, Trash2, Copy } from 'lucide-react'

const EMPTY: Partial<Adapter> = { name: '', slug: '', description: '', isEnabled: true }

export function Adapters() {
  const [adapters, setAdapters] = useState<Adapter[]>([])
  const [formOpen, setFormOpen] = useState(false)
  const [editing, setEditing] = useState<Adapter | null>(null)
  const [form, setForm] = useState<Partial<Adapter>>(EMPTY)
  const [confirmDelete, setConfirmDelete] = useState<Adapter | null>(null)
  const [error, setError] = useState('')
  const [lastStates, setLastStates] = useState<Record<string, string>>({})

  const load = useCallback(() => adaptersApi.list().then(list => {
    setAdapters(list)
    const map: Record<string, string> = {}
    for (const a of list) { if (a.lastState) map[a.id] = a.lastState }
    setLastStates(map)
  }), [])

  const onEventRef = useRef((log: AdapterLog) => {
    if (log.state) setLastStates(prev => ({ ...prev, [log.adapterId]: log.state! }))
  })
  onEventRef.current = (log) => {
    if (log.state) setLastStates(prev => ({ ...prev, [log.adapterId]: log.state! }))
  }
  useLogStream(useCallback(e => {
    if (e.type === 'adapter_log') onEventRef.current(e.data as AdapterLog)
  }, []))
  useEffect(() => { void load() }, [load])

  const openCreate = () => { setForm(EMPTY); setEditing(null); setError(''); setFormOpen(true) }
  const openEdit = (a: Adapter) => { setForm({ ...a }); setEditing(a); setError(''); setFormOpen(true) }

  const save = async () => {
    try {
      if (editing) await adaptersApi.update(editing.id, form)
      else await adaptersApi.create(form)
      setFormOpen(false)
      await load()
    } catch (e) {
      setError(String(e))
    }
  }

  const del = async (a: Adapter) => {
    await adaptersApi.delete(a.id)
    setConfirmDelete(null)
    await load()
  }

  const copyUrl = (slug: string, state?: 'on' | 'off') => {
    const path = state ? `/adapters/in/${slug}/${state}` : `/adapters/in/${slug}`
    void navigator.clipboard.writeText(`${window.location.origin}${path}`)
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">Adapters</h2>
        <Button onClick={openCreate}><Plus size={16} className="mr-2" />New Adapter</Button>
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Name</TableHead>
            <TableHead>Slug / URL</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {adapters.map(a => (
            <TableRow key={a.id}>
              <TableCell>
                <div className="flex items-center gap-2">
                  <span className="font-medium">{a.name}</span>
                  {lastStates[a.id] && (
                    <Badge variant={lastStates[a.id] === 'on' ? 'success' : 'destructive'} className="text-xs">{lastStates[a.id]}</Badge>
                  )}
                </div>
                {a.description && <div className="text-xs text-muted-foreground">{a.description}</div>}
              </TableCell>
              <TableCell>
                <div className="space-y-1">
                  <div className="flex items-center gap-1">
                    <code className="text-xs bg-muted rounded px-1">/adapters/in/{a.slug}/on</code>
                    <Button size="icon" variant="ghost" onClick={() => copyUrl(a.slug, 'on')} title="Copy On URL"><Copy size={12} /></Button>
                  </div>
                  <div className="flex items-center gap-1">
                    <code className="text-xs bg-muted rounded px-1">/adapters/in/{a.slug}/off</code>
                    <Button size="icon" variant="ghost" onClick={() => copyUrl(a.slug, 'off')} title="Copy Off URL"><Copy size={12} /></Button>
                  </div>
                </div>
              </TableCell>
              <TableCell><Badge variant={a.isEnabled ? 'success' : 'secondary'}>{a.isEnabled ? 'Enabled' : 'Disabled'}</Badge></TableCell>
              <TableCell>
                <div className="flex items-center gap-2">
                  <Button size="icon" variant="ghost" onClick={() => openEdit(a)}><Pencil size={14} /></Button>
                  <Button size="icon" variant="ghost" onClick={() => setConfirmDelete(a)}><Trash2 size={14} /></Button>
                </div>
              </TableCell>
            </TableRow>
          ))}
          {adapters.length === 0 && (
            <TableRow><TableCell colSpan={4} className="text-center text-muted-foreground">No adapters yet</TableCell></TableRow>
          )}
        </TableBody>
      </Table>

      <Dialog open={formOpen} onClose={() => setFormOpen(false)} title={editing ? 'Edit Adapter' : 'New Adapter'}>
        <div className="space-y-3">
          {error && <p className="text-sm text-destructive">{error}</p>}
          <div><Label>Name</Label><Input value={form.name ?? ''} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} /></div>
          <div>
            <Label>Slug</Label>
            <Input value={form.slug ?? ''} onChange={e => setForm(f => ({ ...f, slug: e.target.value }))} placeholder="water-level" />
            <p className="text-xs text-muted-foreground mt-1">Endpoints: POST /adapters/in/{form.slug || '<slug>'}/on  •  /off</p>
          </div>
          <div><Label>Description</Label><Input value={form.description ?? ''} onChange={e => setForm(f => ({ ...f, description: e.target.value }))} /></div>
          <div className="flex items-center gap-2"><Switch checked={form.isEnabled ?? true} onCheckedChange={v => setForm(f => ({ ...f, isEnabled: v }))} /><Label>Enabled</Label></div>
          <div className="flex justify-end gap-2">
            <Button variant="outline" onClick={() => setFormOpen(false)}>Cancel</Button>
            <Button onClick={save}>Save</Button>
          </div>
        </div>
      </Dialog>

      <Dialog open={!!confirmDelete} onClose={() => setConfirmDelete(null)} title="Delete Adapter">
        <p className="text-sm mb-4">Delete adapter <strong>{confirmDelete?.name}</strong>? This cannot be undone.</p>
        <div className="flex justify-end gap-2">
          <Button variant="outline" onClick={() => setConfirmDelete(null)}>Cancel</Button>
          <Button variant="destructive" onClick={() => confirmDelete && del(confirmDelete)}>Delete</Button>
        </div>
      </Dialog>
    </div>
  )
}
