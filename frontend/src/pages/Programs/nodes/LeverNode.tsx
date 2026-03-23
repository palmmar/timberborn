import { Handle, Position, useReactFlow } from '@xyflow/react'
import type { NodeProps } from '@xyflow/react'

export interface LeverNodeData {
  leverId: string
  name: string
  [key: string]: unknown
}

export function LeverNode({ id, data }: NodeProps) {
  const { deleteElements } = useReactFlow()
  const d = data as LeverNodeData

  return (
    <div className="relative border-2 border-orange-600 bg-orange-950 rounded-lg px-3 py-2 min-w-[140px] shadow-sm">
      <button
        className="absolute -top-2 -right-2 w-5 h-5 rounded-full bg-red-500 text-white text-xs flex items-center justify-center hover:bg-red-600 leading-none"
        onClick={() => deleteElements({ nodes: [{ id }] })}
      >×</button>
      <div className="text-xs font-semibold text-orange-400 mb-1">LEVER</div>
      <div className="text-sm font-medium text-gray-200 pl-4">{d.name}</div>
      <div className="mt-2 space-y-1">
        <div className="flex items-center justify-start gap-1">
          <Handle
            type="target"
            position={Position.Left}
            id="on"
            style={{ background: '#16a34a', width: 10, height: 10, position: 'relative', top: 'auto', left: 'auto', transform: 'none' }}
          />
          <span className="text-xs text-green-400">on</span>
        </div>
        <div className="flex items-center justify-start gap-1">
          <Handle
            type="target"
            position={Position.Left}
            id="off"
            style={{ background: '#9ca3af', width: 10, height: 10, position: 'relative', top: 'auto', left: 'auto', transform: 'none' }}
          />
          <span className="text-xs text-gray-400">off</span>
        </div>
      </div>
    </div>
  )
}
