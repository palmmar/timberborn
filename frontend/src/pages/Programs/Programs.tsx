import { useEffect, useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { programsApi } from '@/api/client'
import type { AutomationProgramSummary } from '@/api/types'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Dialog } from '@/components/ui/dialog'
import { Switch } from '@/components/ui/switch'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { Plus, Pencil, Trash2 } from 'lucide-react'

export function Programs() {
  const navigate = useNavigate()
  const [programs, setPrograms] = useState<AutomationProgramSummary[]>([])
  const [confirmDelete, setConfirmDelete] = useState<AutomationProgramSummary | null>(null)

  const load = useCallback(() => programsApi.list().then(setPrograms), [])
  useEffect(() => { void load() }, [load])

  const toggleEnabled = async (p: AutomationProgramSummary) => {
    await programsApi.setEnabled(p.id, !p.isEnabled)
    await load()
  }

  const del = async (p: AutomationProgramSummary) => {
    await programsApi.delete(p.id)
    setConfirmDelete(null)
    await load()
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">Programs</h2>
        <Button onClick={() => navigate('/programs/new')}><Plus size={16} className="mr-2" />New Program</Button>
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Name</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Created</TableHead>
            <TableHead>Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {programs.map(p => (
            <TableRow key={p.id}>
              <TableCell className="font-medium">{p.name}</TableCell>
              <TableCell>
                <div className="flex items-center gap-2">
                  <Switch checked={p.isEnabled} onCheckedChange={() => toggleEnabled(p)} />
                  <Badge variant={p.isEnabled ? 'success' : 'secondary'}>{p.isEnabled ? 'Enabled' : 'Disabled'}</Badge>
                </div>
              </TableCell>
              <TableCell className="text-muted-foreground text-sm">{new Date(p.createdAt).toLocaleDateString()}</TableCell>
              <TableCell>
                <div className="flex items-center gap-1">
                  <Button size="icon" variant="ghost" onClick={() => navigate(`/programs/${p.id}`)} title="Edit"><Pencil size={14} /></Button>
                  <Button size="icon" variant="ghost" onClick={() => setConfirmDelete(p)} title="Delete"><Trash2 size={14} /></Button>
                </div>
              </TableCell>
            </TableRow>
          ))}
          {programs.length === 0 && (
            <TableRow>
              <TableCell colSpan={4} className="text-center text-muted-foreground">No programs yet. Create one to get started.</TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>

      <Dialog open={!!confirmDelete} onClose={() => setConfirmDelete(null)} title="Delete Program">
        <p className="text-sm mb-4">Delete program <strong>{confirmDelete?.name}</strong>? This cannot be undone.</p>
        <div className="flex justify-end gap-2">
          <Button variant="outline" onClick={() => setConfirmDelete(null)}>Cancel</Button>
          <Button variant="destructive" onClick={() => confirmDelete && del(confirmDelete)}>Delete</Button>
        </div>
      </Dialog>
    </div>
  )
}
