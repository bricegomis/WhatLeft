import type { Task, CreateTaskRequest, UpdateTaskRequest } from '../types/task'
import type { RecurringTemplate, CreateRecurringTemplateRequest, UpdateRecurringTemplateRequest, RecurrenceType } from '../types/recurring'
import { tokenStorage } from './tokenStorage'

const BASE_URL = __DEV__
  ? 'http://localhost:5000'   // simulator: localhost, physical device: use your LAN IP
  : 'https://your-production-api.example.com'

class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message)
    this.name = 'ApiError'
  }
}

async function request<T>(
  method: string,
  path: string,
  body?: unknown,
): Promise<T> {
  const token = await tokenStorage.get()
  const headers: Record<string, string> = {
    Accept: 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(body ? { 'Content-Type': 'application/json' } : {}),
  }

  const response = await fetch(`${BASE_URL}${path}`, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  })

  if (response.status === 204 || response.headers.get('content-length') === '0') {
    return undefined as T
  }

  const data = await response.json().catch(() => null)

  if (!response.ok) {
    if (response.status === 401) throw new ApiError(401, 'Session expirée')
    throw new ApiError(response.status, data?.message ?? `HTTP ${response.status}`)
  }

  return data as T
}

// ── Tasks ──────────────────────────────────────────────────────────────────

export const tasksApi = {
  getAll: () => request<Task[]>('GET', '/tasks'),
  getHistory: () => request<Task[]>('GET', '/tasks/history'),
  create: (body: CreateTaskRequest) => request<Task>('POST', '/tasks', body),
  update: (id: string, body: UpdateTaskRequest) => request<Task>('PUT', `/tasks/${id}`, body),
  remove: (id: string) => request<void>('DELETE', `/tasks/${id}`),
  reactivate: (id: string) => request<Task>('POST', `/tasks/${id}/reactivate`),
}

// ── Recurring templates ───────────────────────────────────────────────────

export const recurringApi = {
  getAll: () => request<RecurringTemplate[]>('GET', '/recurring-templates'),
  create: (body: CreateRecurringTemplateRequest) =>
    request<RecurringTemplate>('POST', '/recurring-templates', body),
  update: (id: string, body: UpdateRecurringTemplateRequest) =>
    request<RecurringTemplate>('PUT', `/recurring-templates/${id}`, body),
  remove: (id: string) => request<void>('DELETE', `/recurring-templates/${id}`),
  activate: (id: string) => request<void>('POST', `/recurring-templates/${id}/activate`),
  advance: (id: string) => request<void>('POST', `/recurring-templates/${id}/advance`),
  advanceAllByType: (type: RecurrenceType) =>
    request<void>('POST', `/recurring-templates/advance-all?type=${type}`),
  processNow: (id: string) => request<void>('POST', `/recurring-templates/${id}/process`),
}
