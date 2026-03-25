import { useEffect } from 'react'
import type { AdapterLog, ActionLog } from './types'

export interface AdapterLogEvent { type: 'adapter_log'; data: AdapterLog }
export interface ActionLogEvent { type: 'action_log'; data: ActionLog }
export interface SignalUpdateEvent { type: 'signal_update'; data: { programId: string; signals: Record<string, boolean> } }

export type LogStreamEvent = AdapterLogEvent | ActionLogEvent | SignalUpdateEvent

export function useLogStream(onEvent: (e: LogStreamEvent) => void, enabled = true) {
  useEffect(() => {
    if (!enabled) return
    const es = new EventSource('/api/events')
    const handler = (type: LogStreamEvent['type']) => (e: MessageEvent) => {
      try { onEvent({ type, data: JSON.parse(e.data) } as LogStreamEvent) } catch { /* ignore */ }
    }
    es.addEventListener('adapter_log', handler('adapter_log'))
    es.addEventListener('action_log', handler('action_log'))
    es.addEventListener('signal_update', handler('signal_update'))
    return () => es.close()
  }, [enabled, onEvent])
}
