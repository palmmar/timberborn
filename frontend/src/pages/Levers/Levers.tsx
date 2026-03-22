import { useEffect, useState, useCallback } from 'react'
import { leversApi } from '@/api/client'
import type { Lever } from '@/api/types'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Dialog } from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Select } from '@/components/ui/select'
import { Switch } from '@/components/ui/switch'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { Plus, Pencil, Trash2 } from 'lucide-react'

const EMPTY: Partial<Lever> = { name: '', urlOn: '', urlOff: '', httpMethod: 'POST', bodyTemplate: '', description: '', isEnabled: true }

export function Levers() {
  const [levers, setLevers] = useState<Lever[]>([])
  const [formOpen, setFormOpen] = useState(false)
  const [editing, setEditing] = useState<Lever | null>(null)
  const [form, setForm] = useState<Partial<Lever>>(EMPTY)
  const [confirmDelete, setConfirmDelete] = useState<Lever | null>(null)
  const [error, setError] = useState('')

  const load = useCallback(() => leversApi.list().then(setLevers), [])
  useEffect(() => { void load() }, [load])

  const openCreate = () => { setForm(EMPTY); setEditing(null); setError(''); setFormOpen(true) }
  const openEdit = (l: Lever) => { setForm({ ...l }); setEditing(l); setError(''); setFormOpen(true) }

  const save = async () => {
    try {
      if (editing) await leversApi.update(editing.id, form)
      else await leversApi.create(form)
      setFormOpen(false)
      await load()
    } catch (e) {
      setError(String(e))
    }
  }

  const del = async (l: Lever) => {
    await leversApi.delete(l.id)
    setConfirmDelete(null)
    await load()
  }

  const trigger = async (l: Lever, state: 'on' | 'off') => {
    try {
      await leversApi.trigger(l.id, state)
      alert(`Lever triggered (${state}) successfully`)
    } catch (e) {
      alert('Failed: ' + String(e))
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">Levers</h2>
        <Button onClick={openCreate}><Plus size={16} className="mr-2" />New Lever</Button>
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Name</TableHead>
            <TableHead>URL (On)</TableHead>
            <TableHead>URL (Off)</TableHead>
            <TableHead>Method</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {levers.map(l => (
            <TableRow key={l.id}>
              <TableCell className="font-medium">{l.name}</TableCell>
              <TableCell className="max-w-[160px] truncate text-muted-foreground">{l.urlOn ?? '—'}</TableCell>
              <TableCell className="max-w-[160px] truncate text-muted-foreground">{l.urlOff ?? '—'}</TableCell>
              <TableCell><Badge variant="outline">{l.httpMethod}</Badge></TableCell>
              <TableCell><Badge variant={l.isEnabled ? 'success' : 'secondary'}>{l.isEnabled ? 'Enabled' : 'Disabled'}</Badge></TableCell>
              <TableCell>
                <div className="flex items-center gap-1">
                  <Button size="sm" variant="outline" className="text-green-600 border-green-600 hover:bg-green-50 px-2 h-7" onClick={() => trigger(l, 'on')} title="Trigger On">On</Button>
                  <Button size="sm" variant="outline" className="text-orange-600 border-orange-600 hover:bg-orange-50 px-2 h-7" onClick={() => trigger(l, 'off')} title="Trigger Off">Off</Button>
                  <Button size="icon" variant="ghost" onClick={() => openEdit(l)} title="Edit"><Pencil size={14} /></Button>
                  <Button size="icon" variant="ghost" onClick={() => setConfirmDelete(l)} title="Delete"><Trash2 size={14} /></Button>
                </div>
              </TableCell>
            </TableRow>
          ))}
          {levers.length === 0 && (
            <TableRow><TableCell colSpan={6} className="text-center text-muted-foreground">No levers yet</TableCell></TableRow>
          )}
        </TableBody>
      </Table>

      <Dialog open={formOpen} onClose={() => setFormOpen(false)} title={editing ? 'Edit Lever' : 'New Lever'}>
        <div className="space-y-3">
          {error && <p className="text-sm text-destructive">{error}</p>}
          <div><Label>Name</Label><Input value={form.name ?? ''} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} /></div>
          <div><Label>URL (On)</Label><Input value={form.urlOn ?? ''} onChange={e => setForm(f => ({ ...f, urlOn: e.target.value }))} placeholder="http://localhost:8080/lever/on" /></div>
          <div><Label>URL (Off)</Label><Input value={form.urlOff ?? ''} onChange={e => setForm(f => ({ ...f, urlOff: e.target.value }))} placeholder="http://localhost:8080/lever/off" /></div>
          <div>
            <Label>HTTP Method</Label>
            <Select value={form.httpMethod ?? 'POST'} onChange={e => setForm(f => ({ ...f, httpMethod: e.target.value }))}>
              <option>GET</option><option>POST</option><option>PUT</option><option>PATCH</option>
            </Select>
          </div>
          <div><Label>Body Template (JSON)</Label><Textarea rows={3} value={form.bodyTemplate ?? ''} onChange={e => setForm(f => ({ ...f, bodyTemplate: e.target.value }))} /></div>
          <div><Label>Description</Label><Input value={form.description ?? ''} onChange={e => setForm(f => ({ ...f, description: e.target.value }))} /></div>
          <div className="flex items-center gap-2"><Switch checked={form.isEnabled ?? true} onCheckedChange={v => setForm(f => ({ ...f, isEnabled: v }))} /><Label>Enabled</Label></div>
          <div className="flex justify-end gap-2">
            <Button variant="outline" onClick={() => setFormOpen(false)}>Cancel</Button>
            <Button onClick={save}>Save</Button>
          </div>
        </div>
      </Dialog>

      <Dialog open={!!confirmDelete} onClose={() => setConfirmDelete(null)} title="Delete Lever">
        <p className="text-sm mb-4">Delete lever <strong>{confirmDelete?.name}</strong>? This cannot be undone.</p>
        <div className="flex justify-end gap-2">
          <Button variant="outline" onClick={() => setConfirmDelete(null)}>Cancel</Button>
          <Button variant="destructive" onClick={() => confirmDelete && del(confirmDelete)}>Delete</Button>
        </div>
      </Dialog>
    </div>
  )
}
