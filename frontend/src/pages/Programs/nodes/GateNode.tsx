import { Handle, Position, useReactFlow } from '@xyflow/react'
import type { NodeProps } from '@xyflow/react'

export interface GateNodeData {
  inputCount: number
  [key: string]: unknown
}

function GateNodeBase({ id, data, label, borderColor, labelColor }: NodeProps & { label: string; borderColor: string; labelColor: string }) {
  const { deleteElements, updateNodeData } = useReactFlow()
  const d = data as GateNodeData
  const inputCount = d.inputCount ?? 2

  return (
    <div className={`relative border-2 ${borderColor} bg-purple-950 rounded-lg px-3 py-2 min-w-[100px] shadow-sm`}>
      <button
        className="absolute -top-2 -right-2 w-5 h-5 rounded-full bg-red-500 text-white text-xs flex items-center justify-center hover:bg-red-600 leading-none"
        onClick={() => deleteElements({ nodes: [{ id }] })}
      >×</button>
      <div className={`text-xs font-bold ${labelColor} text-center mb-2`}>{label}</div>
      <div className="flex items-stretch gap-3">
        <div className="space-y-2">
          {Array.from({ length: inputCount }, (_, i) => (
            <div key={i} className="flex items-center gap-1">
              <Handle
                type="target"
                position={Position.Left}
                id={`in${i}`}
                style={{ background: '#7c3aed', width: 9, height: 9, position: 'relative', top: 'auto', left: 'auto', transform: 'none' }}
              />
              <span className="text-xs text-purple-400">in{i}</span>
            </div>
          ))}
          <div className="flex items-center gap-1 pt-1">
            <button
              className="text-xs bg-purple-900 hover:bg-purple-800 text-purple-300 rounded px-1"
              onClick={() => updateNodeData(id, { inputCount: inputCount + 1 })}
            >+</button>
            <button
              className="text-xs bg-purple-900 hover:bg-purple-800 text-purple-300 rounded px-1"
              onClick={() => inputCount > 2 && updateNodeData(id, { inputCount: inputCount - 1 })}
            >-</button>
          </div>
        </div>
        <div className="flex items-center">
          <Handle
            type="source"
            position={Position.Right}
            id="out"
            style={{ background: '#7c3aed', width: 9, height: 9, position: 'relative', top: 'auto', right: 'auto', transform: 'none' }}
          />
          <span className="text-xs text-purple-400 ml-1">out</span>
        </div>
      </div>
    </div>
  )
}

export function AndNode(props: NodeProps) {
  return <GateNodeBase {...props} label="AND" borderColor="border-purple-600" labelColor="text-purple-400" />
}

export function OrNode(props: NodeProps) {
  return <GateNodeBase {...props} label="OR" borderColor="border-indigo-600" labelColor="text-indigo-400" />
}
