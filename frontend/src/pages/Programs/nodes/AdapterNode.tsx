import { Handle, Position, useReactFlow } from '@xyflow/react'
import type { NodeProps } from '@xyflow/react'

export interface AdapterNodeData {
  adapterId: string
  name: string
  [key: string]: unknown
}

export function AdapterNode({ id, data }: NodeProps) {
  const { deleteElements } = useReactFlow()
  const d = data as AdapterNodeData

  return (
    <div className="relative border-2 border-blue-400 bg-blue-50 rounded-lg px-3 py-2 min-w-[140px] shadow-sm">
      <button
        className="absolute -top-2 -right-2 w-5 h-5 rounded-full bg-red-500 text-white text-xs flex items-center justify-center hover:bg-red-600 leading-none"
        onClick={() => deleteElements({ nodes: [{ id }] })}
      >×</button>
      <div className="text-xs font-semibold text-blue-700 mb-1">ADAPTER</div>
      <div className="text-sm font-medium text-gray-800 pr-4">{d.name}</div>
      <div className="mt-2 space-y-1">
        <div className="flex items-center justify-end gap-1">
          <span className="text-xs text-green-700">on</span>
          <Handle
            type="source"
            position={Position.Right}
            id="on"
            style={{ background: '#16a34a', width: 10, height: 10, position: 'relative', top: 'auto', right: 'auto', transform: 'none' }}
          />
        </div>
        <div className="flex items-center justify-end gap-1">
          <span className="text-xs text-gray-500">off</span>
          <Handle
            type="source"
            position={Position.Right}
            id="off"
            style={{ background: '#9ca3af', width: 10, height: 10, position: 'relative', top: 'auto', right: 'auto', transform: 'none' }}
          />
        </div>
      </div>
    </div>
  )
}
