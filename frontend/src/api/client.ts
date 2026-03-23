import type {
  Adapter, Lever, AutomationProgram, AutomationProgramSummary, AdapterLog, ActionLog,
  PagedResult, DashboardData
} from './types'

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(path, {
    headers: { 'Content-Type': 'application/json', ...init?.headers },
    ...init,
  })
  if (!res.ok) {
    const body = await res.text()
    throw new Error(`${res.status}: ${body}`)
  }
  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}

// Adapters
export const adaptersApi = {
  list: () => request<Adapter[]>('/api/adapters'),
  get: (id: string) => request<Adapter>(`/api/adapters/${id}`),
  create: (data: Partial<Adapter>) => request<Adapter>('/api/adapters', { method: 'POST', body: JSON.stringify(data) }),
  update: (id: string, data: Partial<Adapter>) => request<Adapter>(`/api/adapters/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  delete: (id: string) => request<void>(`/api/adapters/${id}`, { method: 'DELETE' }),
}

// Levers
export const leversApi = {
  list: () => request<Lever[]>('/api/levers'),
  get: (id: string) => request<Lever>(`/api/levers/${id}`),
  create: (data: Partial<Lever>) => request<Lever>('/api/levers', { method: 'POST', body: JSON.stringify(data) }),
  update: (id: string, data: Partial<Lever>) => request<Lever>(`/api/levers/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  delete: (id: string) => request<void>(`/api/levers/${id}`, { method: 'DELETE' }),
  trigger: (id: string, state: 'on' | 'off') => request<ActionLog>(`/api/levers/${id}/trigger?state=${state}`, { method: 'POST' }),
}

// Programs
export const programsApi = {
  list: () => request<AutomationProgramSummary[]>('/api/programs'),
  get: (id: string) => request<AutomationProgram>(`/api/programs/${id}`),
  create: (data: { name: string; isEnabled: boolean; graphJson: string }) =>
    request<AutomationProgram>('/api/programs', { method: 'POST', body: JSON.stringify(data) }),
  update: (id: string, data: { name: string; isEnabled: boolean; graphJson: string }) =>
    request<AutomationProgram>(`/api/programs/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  delete: (id: string) => request<void>(`/api/programs/${id}`, { method: 'DELETE' }),
  setEnabled: (id: string, isEnabled: boolean) =>
    request<AutomationProgram>(`/api/programs/${id}/enabled`, { method: 'PATCH', body: JSON.stringify({ isEnabled }) }),
}

// Logs
export const logsApi = {
  adapterLogs: (params?: { adapterId?: string; page?: number; pageSize?: number }) => {
    const q = new URLSearchParams()
    if (params?.adapterId) q.set('adapterId', params.adapterId)
    if (params?.page) q.set('page', String(params.page))
    if (params?.pageSize) q.set('pageSize', String(params.pageSize))
    return request<PagedResult<AdapterLog>>(`/api/logs/adapter?${q}`)
  },
  adapterLog: (id: string) => request<AdapterLog>(`/api/logs/adapter/${id}`),
  purgeAdapterLogs: () => request<void>('/api/logs/adapter', { method: 'DELETE' }),
  actionLogs: (params?: { leverId?: string; source?: string; page?: number; pageSize?: number }) => {
    const q = new URLSearchParams()
    if (params?.leverId) q.set('leverId', params.leverId)
    if (params?.source) q.set('source', params.source)
    if (params?.page) q.set('page', String(params.page))
    if (params?.pageSize) q.set('pageSize', String(params.pageSize))
    return request<PagedResult<ActionLog>>(`/api/logs/action?${q}`)
  },
  actionLog: (id: string) => request<ActionLog>(`/api/logs/action/${id}`),
  purgeActionLogs: () => request<void>('/api/logs/action', { method: 'DELETE' }),
}

// Dashboard
export const dashboardApi = {
  get: () => request<DashboardData>('/api/dashboard'),
}
