import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams, Link } from 'react-router-dom'
import {
  ReactFlow, Background, Controls, MiniMap,
  useNodesState, useEdgesState, addEdge,
  ReactFlowProvider,
} from '@xyflow/react'
import type { Connection, Edge, Node } from '@xyflow/react'
import '@xyflow/react/dist/style.css'

import { adaptersApi, leversApi, programsApi } from '@/api/client'
import type { Adapter, Lever } from '@/api/types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { AdapterNode } from './nodes/AdapterNode'
import { LeverNode } from './nodes/LeverNode'
import { AndNode, OrNode } from './nodes/GateNode'
import { NotNode } from './nodes/NotNode'
import { ArrowLeft, Save } from 'lucide-react'

const nodeTypes = {
  adapterNode: AdapterNode,
  leverNode: LeverNode,
  andNode: AndNode,
  orNode: OrNode,
  notNode: NotNode,
}

function ProgramEditorInner() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const isNew = !id || id === 'new'

  const [name, setName] = useState('New Program')
  const [isEnabled, setIsEnabled] = useState(true)
  const [nodes, setNodes, onNodesChange] = useNodesState<Node>([])
  const [edges, setEdges, onEdgesChange] = useEdgesState<Edge>([])
  const [adapters, setAdapters] = useState<Adapter[]>([])
  const [levers, setLevers] = useState<Lever[]>([])
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    void adaptersApi.list().then(setAdapters)
    void leversApi.list().then(setLevers)
  }, [])

  useEffect(() => {
    if (!isNew && id) {
      programsApi.get(id).then(p => {
        setName(p.name)
        setIsEnabled(p.isEnabled)
        try {
          const graph = JSON.parse(p.graphJson)
          if (graph.nodes) setNodes(graph.nodes)
          if (graph.edges) setEdges(graph.edges)
        } catch { /* empty graph */ }
      }).catch(() => navigate('/programs'))
    }
  }, [id, isNew, navigate, setNodes, setEdges])

  const onConnect = useCallback(
    (params: Connection) => setEdges(eds => addEdge({ ...params, type: 'smoothstep' }, eds)),
    [setEdges]
  )

  const onDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    e.dataTransfer.dropEffect = 'move'
  }, [])

  const onDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    const dataStr = e.dataTransfer.getData('application/reactflow')
    if (!dataStr) return

    const { type, data } = JSON.parse(dataStr) as { type: string; data: Record<string, unknown> }

    const rect = (e.currentTarget as HTMLElement).getBoundingClientRect()
    const position = {
      x: e.clientX - rect.left - 70,
      y: e.clientY - rect.top - 30,
    }

    const newNode: Node = {
      id: `${type}-${Date.now()}`,
      type,
      position,
      data,
    }
    setNodes(nds => [...nds, newNode])
  }, [setNodes])

  const addGateNode = (type: 'andNode' | 'orNode' | 'notNode') => {
    const newNode: Node = {
      id: `${type}-${Date.now()}`,
      type,
      position: { x: 200, y: 100 + nodes.length * 60 },
      data: type === 'notNode' ? {} : { inputCount: 2 },
    }
    setNodes(nds => [...nds, newNode])
  }

  const save = async () => {
    setSaving(true)
    setError('')
    try {
      const graphJson = JSON.stringify({ nodes, edges })
      if (isNew) {
        const created = await programsApi.create({ name, isEnabled, graphJson })
        navigate(`/programs/${created.id}`, { replace: true })
      } else {
        await programsApi.update(id!, { name, isEnabled, graphJson })
      }
    } catch (e) {
      setError(String(e))
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="flex flex-col h-screen">
      {/* Top bar */}
      <div className="flex items-center gap-3 px-4 py-2 border-b bg-white shrink-0">
        <Link to="/programs" className="flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground">
          <ArrowLeft size={14} /> Back
        </Link>
        <div className="w-px h-5 bg-border" />
        <Input
          value={name}
          onChange={e => setName(e.target.value)}
          className="w-56 h-8"
          placeholder="Program name"
        />
        <label className="flex items-center gap-2 text-sm">
          <input type="checkbox" checked={isEnabled} onChange={e => setIsEnabled(e.target.checked)} />
          Enabled
        </label>
        <div className="flex items-center gap-1 border rounded px-2 py-1">
          <span className="text-xs text-muted-foreground mr-1">Gates:</span>
          <Button size="sm" variant="outline" className="h-6 px-2 text-xs" onClick={() => addGateNode('andNode')}>AND</Button>
          <Button size="sm" variant="outline" className="h-6 px-2 text-xs" onClick={() => addGateNode('orNode')}>OR</Button>
          <Button size="sm" variant="outline" className="h-6 px-2 text-xs" onClick={() => addGateNode('notNode')}>NOT</Button>
        </div>
        <div className="flex-1" />
        {error && <span className="text-xs text-destructive">{error}</span>}
        <Button size="sm" onClick={save} disabled={saving}>
          <Save size={14} className="mr-1" />{saving ? 'Saving…' : 'Save'}
        </Button>
      </div>

      {/* Canvas area */}
      <div className="flex flex-1 overflow-hidden">
        {/* Left panel: adapters */}
        <div className="w-44 border-r bg-gray-50 overflow-y-auto shrink-0 p-2">
          <div className="text-xs font-semibold text-muted-foreground mb-2 uppercase tracking-wide">Adapters</div>
          {adapters.filter(a => a.isEnabled).map(a => (
            <div
              key={a.id}
              draggable
              onDragStart={e => e.dataTransfer.setData('application/reactflow', JSON.stringify({
                type: 'adapterNode',
                data: { adapterId: a.id, name: a.name }
              }))}
              className="mb-1 cursor-grab border-2 border-blue-300 bg-blue-50 rounded px-2 py-1 text-xs text-blue-800 hover:bg-blue-100 select-none"
            >
              {a.name}
            </div>
          ))}
          {adapters.filter(a => a.isEnabled).length === 0 && (
            <div className="text-xs text-muted-foreground">No adapters</div>
          )}
        </div>

        {/* Flow canvas */}
        <div className="flex-1 relative">
          <ReactFlow
            nodes={nodes}
            edges={edges}
            onNodesChange={onNodesChange}
            onEdgesChange={onEdgesChange}
            onConnect={onConnect}
            onDrop={onDrop}
            onDragOver={onDragOver}
            nodeTypes={nodeTypes}
            fitView
            deleteKeyCode="Delete"
          >
            <Background />
            <Controls />
            <MiniMap />
          </ReactFlow>
        </div>

        {/* Right panel: levers */}
        <div className="w-44 border-l bg-gray-50 overflow-y-auto shrink-0 p-2">
          <div className="text-xs font-semibold text-muted-foreground mb-2 uppercase tracking-wide">Levers</div>
          {levers.filter(l => l.isEnabled).map(l => (
            <div
              key={l.id}
              draggable
              onDragStart={e => e.dataTransfer.setData('application/reactflow', JSON.stringify({
                type: 'leverNode',
                data: { leverId: l.id, name: l.name }
              }))}
              className="mb-1 cursor-grab border-2 border-orange-300 bg-orange-50 rounded px-2 py-1 text-xs text-orange-800 hover:bg-orange-100 select-none"
            >
              {l.name}
            </div>
          ))}
          {levers.filter(l => l.isEnabled).length === 0 && (
            <div className="text-xs text-muted-foreground">No levers</div>
          )}
        </div>
      </div>
    </div>
  )
}

export function ProgramEditor() {
  return (
    <ReactFlowProvider>
      <ProgramEditorInner />
    </ReactFlowProvider>
  )
}
