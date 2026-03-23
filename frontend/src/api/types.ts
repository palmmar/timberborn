export interface Adapter {
  id: string
  name: string
  slug: string
  description?: string
  isEnabled: boolean
  lastState?: string
  createdAt: string
}

export interface Lever {
  id: string
  name: string
  urlOn?: string
  urlOff?: string
  httpMethod: string
  bodyTemplate?: string
  description?: string
  isEnabled: boolean
  createdAt: string
}

export interface AutomationProgram {
  id: string
  name: string
  isEnabled: boolean
  graphJson: string
  createdAt: string
}

export interface AdapterLog {
  id: string
  adapterId: string
  adapter: Adapter
  rawPayload: string
  receivedAt: string
  triggeredAnyRule: boolean
  state?: string
}

export interface ActionLog {
  id: string
  leverId: string
  lever: Lever
  programId?: string
  adapterLogId?: string
  requestBody?: string
  responseStatusCode?: number
  responseBody?: string
  success: boolean
  errorMessage?: string
  calledAt: string
  source: string
}

export interface PagedResult<T> {
  items: T[]
  total: number
}

export interface DashboardData {
  counts: {
    adapters: number
    levers: number
    programs: number
    adapterLogs: number
    actionLogs: number
  }
  recentAdapterLogs: AdapterLog[]
  recentActionLogs: ActionLog[]
}

export interface LogEvent {
  type: 'adapter_log' | 'action_log'
  data: AdapterLog | ActionLog
}
