import { Handle, Position, useReactFlow } from '@xyflow/react'
import type { NodeProps } from '@xyflow/react'

export function NotNode({ id }: NodeProps) {
  const { deleteElements } = useReactFlow()

  return (
    <div className="relative border-2 border-slate-600 bg-slate-900 rounded-lg px-3 py-2 min-w-[90px] shadow-sm">
      <button
        className="absolute -top-2 -right-2 w-5 h-5 rounded-full bg-red-500 text-white text-xs flex items-center justify-center hover:bg-red-600 leading-none"
        onClick={() => deleteElements({ nodes: [{ id }] })}
      >×</button>
      <div className="text-xs font-bold text-slate-300 text-center mb-2">NOT</div>
      <div className="flex items-center gap-3">
        <div className="flex items-center gap-1">
          <Handle
            type="target"
            position={Position.Left}
            id="in"
            style={{ background: '#475569', width: 9, height: 9, position: 'relative', top: 'auto', left: 'auto', transform: 'none' }}
          />
          <span className="text-xs text-slate-400">in</span>
        </div>
        <div className="flex items-center gap-1">
          <Handle
            type="source"
            position={Position.Right}
            id="out"
            style={{ background: '#475569', width: 9, height: 9, position: 'relative', top: 'auto', right: 'auto', transform: 'none' }}
          />
          <span className="text-xs text-slate-400">out</span>
        </div>
      </div>
    </div>
  )
}
