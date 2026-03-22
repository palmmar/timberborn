import { useEffect } from 'react'
import type { AdapterLog, ActionLog } from './types'

export interface LogStreamEvent {
  type: 'adapter_log' | 'action_log'
  data: AdapterLog | ActionLog
}

export function useLogStream(onEvent: (e: LogStreamEvent) => void, enabled = true) {
  useEffect(() => {
    if (!enabled) return
    const es = new EventSource('/api/events')
    const handler = (type: 'adapter_log' | 'action_log') => (e: MessageEvent) => {
      try {
        onEvent({ type, data: JSON.parse(e.data) as AdapterLog | ActionLog })
      } catch { /* ignore */ }
    }
    es.addEventListener('adapter_log', handler('adapter_log'))
    es.addEventListener('action_log', handler('action_log'))
    return () => es.close()
  }, [enabled, onEvent])
}
