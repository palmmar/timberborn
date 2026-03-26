import { Handle, Position, useReactFlow } from '@xyflow/react'
import type { NodeProps } from '@xyflow/react'

export function DelayNode({ id, data }: NodeProps) {
  const { deleteElements, updateNodeData } = useReactFlow()
  const seconds = (data.delaySeconds as number) ?? 5

  return (
    <div className="relative border-2 border-yellow-600 bg-yellow-950 rounded-lg px-3 py-2 min-w-[130px] shadow-sm">
      <button
        className="absolute -top-2 -right-2 w-5 h-5 rounded-full bg-red-500 text-white text-xs flex items-center justify-center hover:bg-red-600 leading-none"
        onClick={() => deleteElements({ nodes: [{ id }] })}
      >×</button>
      <div className="text-xs font-bold text-yellow-400 text-center mb-2">DELAY</div>
      <div className="flex items-center gap-2">
        <div className="flex items-center gap-1">
          <Handle
            type="target"
            position={Position.Left}
            id="in"
            style={{ background: '#ca8a04', width: 9, height: 9, position: 'relative', top: 'auto', left: 'auto', transform: 'none' }}
          />
          <span className="text-xs text-yellow-400">in</span>
        </div>
        <div className="flex flex-col items-center gap-0.5 flex-1">
          <input
            type="number"
            min={1}
            max={3600}
            value={seconds}
            onChange={e => updateNodeData(id, { delaySeconds: Math.max(1, parseInt(e.target.value) || 1) })}
            className="w-full text-center text-xs bg-yellow-900 border border-yellow-700 rounded text-yellow-200 nodrag px-1 py-0.5"
          />
          <span className="text-[10px] text-yellow-600">seconds</span>
        </div>
        <div className="flex items-center gap-1">
          <span className="text-xs text-yellow-400">out</span>
          <Handle
            type="source"
            position={Position.Right}
            id="out"
            style={{ background: '#ca8a04', width: 9, height: 9, position: 'relative', top: 'auto', right: 'auto', transform: 'none' }}
          />
        </div>
      </div>
    </div>
  )
}
